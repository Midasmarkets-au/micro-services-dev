use harsh::Harsh;

const PARTY_SALT: &str = "BCRPartyId";
const PARTY_ALPHABET: &str = "ABCDEFGHJKMNPQRSTUVWXYZ0123456789";
const USER_SALT: &str = "BCRUserId";
const USER_ALPHABET: &str = "ABCDEFGHJKMNPQRSTUVWXYZ0123456789";
const MIN_LENGTH: usize = 8;

fn build_harsh(salt: &str, alphabet: &str) -> Harsh {
    Harsh::builder()
        .salt(salt)
        .alphabet(alphabet)
        .length(MIN_LENGTH)
        .build()
        .expect("failed to build Harsh encoder")
}

pub fn encode_party_id(id: i64) -> String {
    let h = build_harsh(PARTY_SALT, PARTY_ALPHABET);
    h.encode(&[id as u64])
}

pub fn encode_user_id(id: i64) -> String {
    let h = build_harsh(USER_SALT, USER_ALPHABET);
    h.encode(&[id as u64])
}
