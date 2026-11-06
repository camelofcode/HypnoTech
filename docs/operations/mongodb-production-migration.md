# Future production MongoDB migration

> **Status:** Deferred planning only. This document does not authorise connecting to or
> changing a production server.

## Objective and decision

Move the production data from the native MongoDB 4.0.3 DigitalOcean One-Click Droplet
to a current, pinned MongoDB container without changing collection names, BSON
types/member names, application behaviour, or the deliberately low steady-state cost.

Prefer a rehearsed blue/green logical migration over upgrading the live database
through every intermediate release:

- create a powered-off DigitalOcean snapshot and use it to clone the source for a safe
  inventory and migration rehearsal;
- create a temporary replacement Droplet on the same 1-vCPU/2-GB class and a supported
  Ubuntu LTS release;
- install Podman and run the pinned official MongoDB image under systemd/Quadlet;
- use a host-visible persistent bind mount such as `/srv/mongodb/data`, with config and
  secrets outside the image;
- copy and verify data during a controlled no-write window, then switch the sites;
- retire the old Droplet after burn-in, leaving no permanent second-server cost.

Podman should add little overhead because it does not introduce a guest OS, but that is
a hypothesis to measure. The current server performs adequately, so the target keeps
the same capacity by default. Compare native and container memory, CPU, disk, and query
latency during rehearsal; reject or tune the container setup if it causes a meaningful
regression. Do not resize speculatively or add resource limits without measuring their
interaction with the WiredTiger cache.

## Migration method

An in-place upgrade from 4.0 would require successive 4.2, 4.4, 5.0, 6.0, 7.0, 8.0,
and 9.0 binary/feature-compatibility steps. Keep that as the fallback, not the preferred
live-server plan.

MongoDB permits raw BSON dumps to be restored to a newer server, but does not recommend
that as a general upgrade method; metadata, archives, and oplog files have additional
cross-version restrictions. Current Database Tools also no longer connect to MongoDB
4.0. Rehearse these two candidates and choose the smallest one that proves complete:

1. A source-compatible `mongodump` to directory-format BSON plus a proven restore-tool
   combination.
2. A one-use C# copier using raw `BsonDocument` values, preserving `_id`, BSON types,
   collection options, and indexes. MongoDB.Driver 3.4.3 is already contract-tested by
   this repository against MongoDB 4.0.3 and 9.0.4.

Do not use ordinary JSON. Create fresh target credentials and roles instead of blindly
copying authentication system collections.

## Future execution plan

### 1. Clone and inventory

- Stop writes and power off the source before taking the snapshot; DigitalOcean warns
  that live database snapshots can be inconsistent.
- Create a same-size clone from the snapshot. It receives a different IP and must be
  isolated so cloned web jobs, email, or database clients cannot affect production.
- Record FCV, storage engine, collections/options, indexes, users/roles, data/index
  size, disk capacity, and every writer. Keep addresses, credentials, and private data
  out of this repository.

### 2. Rehearse

- Restore/copy from the isolated clone into a disposable pinned target container.
- Compare collection/index inventories, counts, deterministic hashes, representative
  BSON types, and application queries.
- Run engine contracts and approved private-site smoke tests without committing private
  data, then compare resource use and time the full migration and rollback.

### 3. Provision the replacement

- Install Podman on the replacement OS and pin the MongoDB image by version and digest,
  never `latest`.
- Bind MongoDB only to localhost or the private network, never the public Internet.
- Configure persistent storage, permissions, restart-on-boot, health checks, log
  rotation, and backups while retaining the existing capacity and swap policy.
- Verify container stop/start, host reboot, backup, and restore before loading live data.

### 4. Cut over

- Stop application writes and take a final provider snapshot plus an independent
  export, recording checksums and source counts.
- Import into an empty target with stop-on-error behaviour; recreate indexes and fresh
  credentials, then repeat every rehearsal check.
- Start the sites against the target with writes still disabled and test read, login,
  CMS, media, and ecommerce paths.
- On any failure, point the sites back to the untouched source. Enable writes only
  after the target is accepted.

The simple rollback point ends when new target writes are accepted. Later rollback
requires reverse-copy/reconciliation or an explicitly accepted loss window; an old,
unchanged server alone is then insufficient.

### 5. Burn in and retire

- Monitor availability, logs, memory/cache pressure, disk growth, latency, and backups.
- Verify a fresh backup/restore from the containerised deployment.
- Retain the old deployment and snapshots for an agreed period; destroy them only with
  separate approval.
- Record the deployed image, driver, and results in `docs/compatibility.md`.

## Future automation requirements

The eventual script should default to inventory/dry-run mode, be idempotent where
practical, redact secrets, write a timestamped local log, and stop for confirmation at
backup, cutover, rollback, and retirement boundaries. It must not combine host
replacement, copy, cutover, and old-host deletion into one irreversible command.

## References

- [MongoDB upgrade paths](https://www.mongodb.com/docs/mongodb-versions/)
- [`mongorestore` cross-version behaviour](https://www.mongodb.com/docs/database-tools/mongorestore/mongorestore-behavior-access-usage/)
- [MongoDB Database Tools compatibility](https://www.mongodb.com/docs/database-tools/)
- [MongoDB production notes](https://www.mongodb.com/docs/manual/administration/production-notes/)
- [MongoDB Community platform support](https://www.mongodb.com/docs/community-platform-support/)
- [Podman installation](https://podman.io/docs/installation)
- [DigitalOcean: clone a Droplet](https://docs.digitalocean.com/support/how-do-i-clone-or-make-a-copy-of-a-droplet/)
- [DigitalOcean: create a consistent snapshot](https://docs.digitalocean.com/products/snapshots/how-to/snapshot-droplets/)
