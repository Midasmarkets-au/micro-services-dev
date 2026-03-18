#![allow(dead_code)]

use anyhow::Result;
use serde::Serialize;

/// Write a slice of serializable records to CSV bytes.
pub fn to_csv_bytes<T: Serialize>(records: &[T]) -> Result<Vec<u8>> {
    let mut buf = Vec::new();
    {
        let mut wtr = csv::Writer::from_writer(&mut buf);
        for record in records {
            wtr.serialize(record)?;
        }
        wtr.flush()?;
    }
    Ok(buf)
}

/// Write records with a custom header row.
pub fn to_csv_bytes_with_headers<T: Serialize>(
    headers: &[&str],
    records: &[T],
) -> Result<Vec<u8>> {
    let mut buf = Vec::new();
    {
        let mut wtr = csv::WriterBuilder::new()
            .has_headers(false)
            .from_writer(&mut buf);
        // Write custom header
        wtr.write_record(headers)?;
        for record in records {
            wtr.serialize(record)?;
        }
        wtr.flush()?;
    }
    Ok(buf)
}
