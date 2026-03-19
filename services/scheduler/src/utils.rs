use chrono::{DateTime, Datelike, Duration, NaiveDate, Utc, Weekday};

/// Return the nth occurrence of `weekday` in the given year/month (1-indexed).
fn nth_weekday(year: i32, month: u32, weekday: Weekday, n: u32) -> NaiveDate {
    let first = NaiveDate::from_ymd_opt(year, month, 1).unwrap();
    let offset =
        (weekday.num_days_from_monday() + 7 - first.weekday().num_days_from_monday()) % 7;
    first + Duration::days(offset as i64) + Duration::weeks((n - 1) as i64)
}

/// Returns `true` if `dt` falls within US Pacific Daylight Time (PDT, UTC-7).
///
/// Exact US DST rules:
///   Begins: 2nd Sunday of March  at 02:00 PST (UTC-8) = **10:00 UTC**
///   Ends:   1st Sunday of November at 02:00 PDT (UTC-7) = **09:00 UTC**
///
/// This replaces the previous month-range approximation `(3..=11).contains(&month)`
/// which was off by ~1 week at each transition boundary.
pub fn is_dst_los_angeles(dt: DateTime<Utc>) -> bool {
    let y = dt.year();

    let start_naive = nth_weekday(y, 3, Weekday::Sun, 2)
        .and_hms_opt(10, 0, 0)
        .unwrap();
    let end_naive = nth_weekday(y, 11, Weekday::Sun, 1)
        .and_hms_opt(9, 0, 0)
        .unwrap();

    let start = DateTime::<Utc>::from_naive_utc_and_offset(start_naive, Utc);
    let end = DateTime::<Utc>::from_naive_utc_and_offset(end_naive, Utc);

    dt >= start && dt < end
}
