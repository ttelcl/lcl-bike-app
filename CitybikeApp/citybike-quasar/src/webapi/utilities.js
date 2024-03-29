export const utilities = {
  // Based on https://www.30secondsofcode.org/js/s/to-iso-string-with-timezone.
  // Convert a date using its existing timezone to a yyyy-MM-dd string,
  // *without converting it to UTC first*.
  toIsoLikeDateString(date) {
    const pad = (n) => `${Math.floor(Math.abs(n))}`.padStart(2, "0");
    return (
      date.getFullYear() +
      "-" +
      pad(date.getMonth() + 1) +
      "-" +
      pad(date.getDate())
    );
  },

  // From https://stackoverflow.com/a/17415677/271323
  toIsoString(date) {
    var tzo = -date.getTimezoneOffset(),
      dif = tzo >= 0 ? "+" : "-",
      pad = function (num) {
        return (num < 10 ? "0" : "") + num;
      };

    return (
      date.getFullYear() +
      "-" +
      pad(date.getMonth() + 1) +
      "-" +
      pad(date.getDate()) +
      "T" +
      pad(date.getHours()) +
      ":" +
      pad(date.getMinutes()) +
      ":" +
      pad(date.getSeconds()) +
      dif +
      pad(Math.floor(Math.abs(tzo) / 60)) +
      ":" +
      pad(Math.abs(tzo) % 60)
    );
  },

  formatTimespan(totalSeconds) {
    if (Number.isFinite(totalSeconds)) {
      var rounded = Math.floor(totalSeconds);
      var seconds = rounded % 60;
      var totalMinutes = (rounded - seconds) / 60;
      var minutes = totalMinutes % 60;
      var hours = (totalMinutes - minutes) / 60;
      return (
        hours.toString() +
        ":" +
        minutes.toString().padStart(2, "0") +
        ":" +
        seconds.toString().padStart(2, "0")
      );
    } else {
      return "";
    }
  },

  parseTimespan(txt) {
    if (txt) {
      // (/^(?:(?:(?<hr>\d+):)?(?<min>\d+):)?(?<sec>\d+)$/.exec("12:34:56") || {}).groups;
      // For validation only: /^(?:(?:(\d+):)?(\d+):)?(\d+)$/
      const match = /^(?:(?:(?<hr>\d+):)?(?<min>\d+):)?(?<sec>\d+)$/.exec(txt);
      if (match) {
        const hours = parseInt(match.groups.hr || "0", 10);
        const minutes = parseInt(match.groups.min || "0", 10);
        const seconds = parseInt(match.groups.sec || "0", 10);
        return seconds + 60 * (minutes + 60 * hours);
      } else {
        return null;
      }
    } else {
      return null;
    }
  },
};
