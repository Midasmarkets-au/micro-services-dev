export default {
  getDateAndTimeFromISOString: (dateString: string) => {
    const date = new Date(dateString);

    const year = date.getFullYear();
    const month = date.getMonth() + 1; // add 1 because getMonth() returns a zero-based index
    const day = date.getDate();
    let hour = date.getHours();
    const minute = date.getMinutes();

    let period = "AM";
    if (hour >= 12) {
      period = "PM";
      hour -= 12;
    }

    return `${year}-${month}-${day} ${hour}:${minute} ${period}`;
  },
};

export const convertToUTC = (period) => {
  if (period[1]) {
    period[1] = period[1].replace(
      /(\d{4}-\d{2}-\d{2}) \d{2}:\d{2}:\d{2}/,
      "$1 23:59:59"
    );
  }
  const val = period.map((x) => (x = x.replace(" ", "T") + "Z"));
  return { from: val ? val[0] : null, to: val ? val[1] : null };
};
