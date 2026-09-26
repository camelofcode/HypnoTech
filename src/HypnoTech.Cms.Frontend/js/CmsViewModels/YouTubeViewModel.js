function YouTubeViewModel(id) {
    "use strict";
    var self = this;

    self.youTubeId = ko.observable(id);

    self.youTubeUrl = ko.pureComputed({
        read: function() {
            return self.youTubeId() ? "https://www.youtube.com/watch?v=" + self.youTubeId() : null;
        },
        write: function(url) {
            var match = url.match(/^.*(?:(?:youtu\.be\/|v\/|vi\/|u\/\w\/|embed\/)|(?:(?:watch)?\?v(?:i)?=|\&v(?:i)?=))([^#\&\?]*).*/);
            if (match && match[1]) {
                self.youTubeId(match[1]);
            } else {
                self.youTubeId(null);
            }

        },
        owner: self
    });

    self.hasId = ko.pureComputed(function() {
        return self.youTubeId() !== null && self.youTubeId() !== null && self.youTubeId() !== "";
    });

    self.thumbnail = ko.pureComputed(function() {
        if (self.hasId()) {
            return "https://img.youtube.com/vi/" + self.youTubeId() + "/0.jpg";
        }
        return "";
    });
}