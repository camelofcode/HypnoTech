using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Common;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Cms.Contracts.Dtos.Response;
using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.MongoDb;

namespace HypnoTech.Cms.MongoDb.Repositories
{
    public class CmsRepository<TClass> : ICmsRepository<TClass> where TClass : class
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IMongoDatabase _database;
        public CmsRepository(IOptions<MongoOptions> config)
        {
            var client = new MongoClient(config.Value.ConnectionString);

            _database = client.GetDatabase(config.Value.Database);
            var collection = FormClassMapping.GetMapping<TClass>().DataSource;
            _collection = _database.GetCollection<BsonDocument>(collection);
        }

        public void MoveUp(string id)
        {
            Move(id, -1);
        }

        public void MoveDown(string id)
        {
            Move(id, 1);
        }

        private void Move(string id, int newPosition)
        {
            var current = GetById(id);

            var mapping = FormClassMapping.GetMapping<TClass>();
            var currentOrder = (int?)typeof(TClass).GetProperty(mapping.OrderProperty)?.GetValue(current);

            if (!currentOrder.HasValue)
            {
                throw new Exception("Order property could not be evaluated as an integer!");
            }

            var newOrder = currentOrder + newPosition;

            var itemToSwapDocument = _collection
                .Find(new BsonDocument(GetOrderColumnMongoName<TClass>(), new BsonInt32(newOrder.Value)))
                .FirstOrDefault();
            if (itemToSwapDocument == null)
            {
                return;
            }

            var itemToSwap = BsonSerializer.Deserialize<TClass>(itemToSwapDocument);

            typeof(TClass).GetProperty(mapping.OrderProperty)?.SetValue(current, newOrder);
            typeof(TClass).GetProperty(mapping.OrderProperty)?.SetValue(itemToSwap, currentOrder);

            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(mapping.GetId(current)));
            _collection.ReplaceOne(filter, current.ToBsonDocument());

            filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(mapping.GetId(itemToSwap)));
            _collection.ReplaceOne(filter, itemToSwap.ToBsonDocument());
        }

        public TClass GetById(string id)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            var item = GetOne(new BsonDocument("_id", new BsonObjectId(new ObjectId(id))));

            PopulateChildren(mapping, item);

            return item;
        }

        public void Add(TClass model)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            mapping.SetId(model, ObjectId.GenerateNewId().ToString());

            SetListItemIds(model, mapping);

            _collection.InsertOne(model.ToBsonDocument());
        }

        public void Edit(TClass model)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(mapping.GetId(model)));

            var original = GetById(mapping.GetId(model));
            SetListItemIds(model, mapping);

            _collection.ReplaceOne(filter, model.ToBsonDocument());
        }

        public void Remove(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            _collection.DeleteOne(filter);
        }

        public ICollection<TClass> GetAll<T>(string orderBy = null, bool descending = false)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();

            var collection = _collection.Find(new BsonDocument());
            collection = Order(collection, orderBy, descending);
            var items = collection.ToList().Select(p => BsonSerializer.Deserialize<TClass>(p)).ToList();

            foreach (var item in items)
            {
                PopulateChildren(mapping, item);
            }

            return items;
        }

        public CmsListReponse<TClass> GetPaged<T>(CmsListRequest request)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            var search = BuildSearch(request);
            List<TClass> items;

            ApplyDefaultSortIfUnset(request, mapping);

            if (mapping.Ordered)
            {
                var collection = Order(_collection.Find(search), GetOrderColumnMongoName<TClass>())
                    .Skip(request.Skip)
                    .Limit(request.PageSize);

                items = collection.ToList().Select(p => BsonSerializer.Deserialize<TClass>(p)).ToList();
            }
            else
            {
                var collection = Order(_collection.Find(search), GetMongoColumnName<TClass>(request), request.IsDescending)
                    .Skip(request.Skip)
                    .Limit(request.PageSize);

                items = collection.ToList().Select(p => BsonSerializer.Deserialize<TClass>(p)).ToList();
            }


            foreach (var item in items)
            {
                PopulateChildren(mapping, item);
            }

            return new CmsListReponse<TClass>
            {
                SortColumn = request.SortColumn ?? 0,
                Direction = request.Direction,
                Search = request.Search,
                CurrentPage = request.Page,
                Items = items.ToList(),
                TotalPages = (int)Math.Ceiling((_collection.CountDocuments(search) / (decimal)request.PageSize))
            };
        }

        private static void ApplyDefaultSortIfUnset(CmsListRequest request, FormClassMapping<TClass> mapping)
        {
            if (!request.SortColumn.HasValue)
            {
                if (string.IsNullOrEmpty(mapping.SortProperty))
                {
                    request.SortColumn = 0;
                }
                else
                {
                    var member = mapping.GetMember(mapping.SortProperty);
                    var index = member.TableOrder ?? mapping.MemberMappings.ToList().IndexOf(member);
                    request.SortColumn = index;
                    request.Direction = mapping.SortAscending ? SortType.Ascending : SortType.Descending;
                }
            }
        }

        private IFindFluent<BsonDocument, BsonDocument> Order(IFindFluent<BsonDocument, BsonDocument> collection, string orderBy = null, bool descending = false)
        {
            if (!string.IsNullOrEmpty(orderBy))
            {
                return descending ? collection.SortByDescending(bson => bson[orderBy]) : collection.SortBy(bson => bson[orderBy]);
            }

            return collection;
        }

        private TClass GetOne(FilterDefinition<BsonDocument> filter)
        {
            var document = _collection.Find(filter).FirstOrDefault();

            return BsonSerializer.Deserialize<TClass>(document);
        }

        private FilterDefinition<BsonDocument> BuildSearch(CmsListRequest request)
        {
            var builder = Builders<BsonDocument>.Filter;

            if (request.HasSearch)
            {
                var filters = new List<FilterDefinition<BsonDocument>>();

                foreach (var member in FormClassMapping.GetMapping<TClass>().MemberMappings.Where(p => p.IsSearchable))
                {
                    var bsonClassMap = BsonClassMap.LookupClassMap(typeof(TClass));

                    var element = bsonClassMap.GetMemberMap(member.MemberInfo.Name);
                    //TODO: This crashes if there are nested members and we don't check element for null.
                    //Should support this in future, for now it just won't search the nested elements.
                    if (element != null)
                    {
                        filters.Add(builder.Regex(element.ElementName, new BsonRegularExpression(Regex.Escape(request.Search), "i")));
                    }
                }
                if (filters.Count == 1)
                {
                    return filters.FirstOrDefault();
                }
                if (filters.Count == 0)
                {
                    return builder.Exists("_id", false);
                }
                return builder.Or(filters.ToArray());
            }

            return new BsonDocument();
        }

        private string GetMongoColumnName<T>(CmsListRequest request)
        {
            var formClassMap = FormClassMapping.GetMapping<TClass>();
            var column = formClassMap.MemberMappings.FirstOrDefault(p => p.TableOrder == request.SortColumn);
            var bsonClassMap = BsonClassMap.LookupClassMap(typeof(TClass));
            var member = bsonClassMap.GetMemberMap(column.MemberInfo.Name);

            //If request doesn't specify order default asc/desc depending on type
            if (request.Direction == SortType.Unsorted)
            {
                request.Direction = column.MemberType == typeof(DateTime) || column.MemberType == typeof(DateTime?)
                    ? SortType.Descending
                    : SortType.Ascending;
            }

            return member.ElementName;
        }

        private string GetOrderColumnMongoName<T>()
        {
            var formClassMap = FormClassMapping.GetMapping<TClass>();
            var bsonClassMap = BsonClassMap.LookupClassMap(typeof(TClass));
            var member = bsonClassMap.GetMemberMap(formClassMap.OrderProperty);

            return member.ElementName;
        }

        private void PopulateChildren(FormClassMapping<TClass> mapping, TClass item)
        {
            foreach (var child in mapping.Children)
            {
                var id = typeof(TClass).GetProperty(child.IdInfo.Name)?.GetValue(item)?.ToString();
                if (!string.IsNullOrEmpty(id))
                {
                    var childCollection = _database.GetCollection<BsonDocument>(child.DataStore);
                    var childObject = childCollection.Find(new BsonDocument("_id", new BsonObjectId(new ObjectId(id)))).FirstOrDefault();
                    MethodInfo deserialize = GetGenericDeserializer(child.Type);
                    typeof(TClass).GetProperty(child.TypeInfo.Name)?.SetValue(item, deserialize?.Invoke(null, new object[] { childObject, null }));
                }
            }
        }

        private MethodInfo GetGenericDeserializer(Type type)
        {
            foreach (var method in typeof(BsonSerializer).GetMethods().Where(p => p.Name == "Deserialize"))
            {
                if (method.GetParameters()[0].ParameterType == typeof(BsonDocument))
                {
                    return method.MakeGenericMethod(type);
                }
            }

            return null;
        }

        private static void SetListItemIds(TClass model, FormClassMapping<TClass> mapping)
        {
            foreach (var member in mapping.MemberMappings.Where(p => p is ListInputMapping))
            {
                var listMember = (ListInputMapping)member;
                listMember.SetIds(member.GetPropertyValue(model), () => ObjectId.GenerateNewId().ToString());
            }
            foreach (var member in mapping.MemberMappings.Where(p => p is PhotoGalleryInputMapping))
            {
                var listMember = (PhotoGalleryInputMapping)member;
                listMember.SetIds(member.GetPropertyValue(model), () => ObjectId.GenerateNewId().ToString());
            }
        }
    }
}
