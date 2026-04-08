/// tonic gRPC client — calls idgen's ApiService to generate Snowflake IDs.
use tonic::transport::Channel;

use crate::generated::api::v1::{api_service_client::ApiServiceClient, GenerateIdRequest};

#[derive(Clone)]
pub struct IdgenClient {
    endpoint: String,
}

impl IdgenClient {
    pub fn new(url: &str) -> Self {
        Self {
            endpoint: url.to_string(),
        }
    }

    /// 从 idgen 获取一个 Snowflake ID。
    /// snowflaked 使用 UNIX_EPOCH，生成的 u64 高位始终为 0（有效至 2262 年），可安全转为 i64。
    pub async fn generate_id(&self) -> anyhow::Result<i64> {
        let channel = Channel::from_shared(self.endpoint.clone())?.connect().await?;
        let mut client = ApiServiceClient::new(channel);
        let resp = client
            .generate_id(GenerateIdRequest { work_id: 0 })
            .await?;
        Ok(resp.into_inner().id as i64)
    }
}
