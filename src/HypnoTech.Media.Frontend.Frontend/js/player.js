//player.js
function TrackViewModel(data) {
    "use strict";
    var self = this;

    self.id = ko.observable(data.id);
    self.title = ko.observable(data.title);
    self.path = ko.observable(data.path);
    self.imagePath = ko.observable(data.imagePath);
    self.href = ko.observable(data.url);
}

function PlayerViewModel(tracks) {
    "use strict";
    var self = this;

    self.playing = ko.observable(false);
    self.position = ko.observable(0);
    self.positionPercent = ko.pureComputed(function () {
        return self.position() + "%";
    });

    self.currentTrackIndex = ko.observable(0);

    self.tracks = ko.observableArray([]);

    for (var i = 0; i < tracks.length; i++) {
        self.tracks.push(new TrackViewModel(tracks[i]));
    }

    var audioElement = document.getElementById("audio-element");

    audioElement.addEventListener("playing", function () {
        self.playing(true);

    }, true);

    audioElement.addEventListener("pause", function () {
        self.playing(false);

    }, true);

    audioElement.addEventListener("timeupdate", function () {
        var currentTime = audioElement.currentTime,
            duration = audioElement.duration,
            position = Math.round((currentTime / duration) * 100);

        if (self.position() !== position) {
            self.position(position);
        }
    }, true);

    audioElement.addEventListener("ended", function () {
        var next = self.currentTrackIndex() + 1;

        if (next >= self.tracks().length) {
            next = 0;
        }
        self.selectTrack(self.tracks()[next]);
    }, true);

    self.currentTrack = ko.pureComputed(function () {
        return self.tracks()[self.currentTrackIndex()];
    });

    self.play = function () {
        audioElement.play();
    };

    self.pause = function () {
        audioElement.pause();
    };

    self.isCurrent = function (track) {
        return self.currentTrackIndex() === self.tracks().indexOf(track);
    };

    self.selectTrack = function (track) {
        self.currentTrackIndex(self.tracks().indexOf(track));

        setTimeout(function () {
            audioElement.load();
            audioElement.play();
        }, 100);
    };

    self.setPosition = function (data, event) {
        if (self.playing()) {
            var barWidth = document.getElementById("progress-track").offsetWidth,
                clickPosition = event.offsetX || event.originalEvent.layerX,
                clickPercent = clickPosition / barWidth,
                newPosition = Math.round(audioElement.duration * clickPercent);

            audioElement.currentTime = newPosition;
        }
    };

    events.on("player:playTrack", function (id) {
        for (var j = 0; j < self.tracks().length; j++) {
            if (self.tracks()[j].id() === id) {
                self.selectTrack(self.tracks()[j]);
                break;
            }
        }
    });
}  