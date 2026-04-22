/// Deserialize a JSON string or number into a `String`.
///
/// The mono (.NET) `RegistrationRequest.CCC` field is typed `string`, but the
/// Vue frontend historically sent the dial-code as a bare JSON number (e.g.
/// `"ccc": 61`).  ASP.NET's JSON deserializer accepted this silently; serde's
/// strict mode returns 422.  This helper restores the lenient behaviour.
pub mod string_or_number {
    use serde::{Deserialize, Deserializer};

    pub fn deserialize<'de, D>(deserializer: D) -> Result<String, D::Error>
    where
        D: Deserializer<'de>,
    {
        #[derive(Deserialize)]
        #[serde(untagged)]
        enum StringOrNumber {
            Str(String),
            Int(i64),
            Float(f64),
        }

        match StringOrNumber::deserialize(deserializer)? {
            StringOrNumber::Str(s) => Ok(s),
            StringOrNumber::Int(n) => Ok(n.to_string()),
            StringOrNumber::Float(f) => Ok(f.to_string()),
        }
    }
}
