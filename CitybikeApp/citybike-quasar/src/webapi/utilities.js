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
};
