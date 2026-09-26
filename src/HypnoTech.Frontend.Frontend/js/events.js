(function (root, factory) {
    'use strict';

    if (typeof define === 'function' && define.amd) {
        define([], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory();
    } else {
        root.events = factory();
    }
}(this, function () {
    'use strict';

    var queue = {};

    function EventHandler(callback) {
        this.trigger = function (args) {
            callback(args);
        };
    }

    function processEventQueue(eventQueue, args) {
        var executedHandlers = [];

        var i;
        var handler;
        for (i = 0; i < eventQueue.length; i++) {
            handler = eventQueue[i];

            handler.trigger(args);
            executedHandlers.push(handler);
        }

        return executedHandlers;
    }

    return {
        trigger: function (eventName, args, target) {
            var eventQueue = queue[eventName];
            if (!utilities.exists(eventQueue)) {
                return;
            }

            processEventQueue(eventQueue, args, target, false);
        },

        on: function (eventName, callback) {
            if (!utilities.exists(queue[eventName])) {
                queue[eventName] = [];
            }

            var handler = new EventHandler(callback, eventName);
            queue[eventName].push(handler);

            return handler;
        }
    };
}));