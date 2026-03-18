use anyhow::Result;
use aws_config::BehaviorVersion;
use aws_sdk_s3::Client;
use aws_sdk_s3::config::{Credentials, Region};
use aws_sdk_s3::primitives::ByteStream;

use crate::config::Config;

#[derive(Clone)]
pub struct S3Storage {
    client: Client,
    bucket: String,
}

impl S3Storage {
    pub async fn new(config: &Config) -> Result<Self> {
        let creds = Credentials::new(
            &config.s3_access_key,
            &config.s3_secret_key,
            None,
            None,
            "report-service",
        );

        let mut builder = aws_config::defaults(BehaviorVersion::latest())
            .region(Region::new(config.s3_region.clone()))
            .credentials_provider(creds);

        if let Some(endpoint) = &config.s3_endpoint {
            builder = builder.endpoint_url(endpoint);
        }

        let aws_config = builder.load().await;
        let client = Client::new(&aws_config);

        Ok(Self {
            client,
            bucket: config.s3_bucket.clone(),
        })
    }

    /// Upload bytes to S3, return the key (path).
    pub async fn upload(&self, key: &str, data: Vec<u8>, content_type: &str) -> Result<String> {
        self.client
            .put_object()
            .bucket(&self.bucket)
            .key(key)
            .body(ByteStream::from(data))
            .content_type(content_type)
            .send()
            .await?;
        Ok(key.to_string())
    }

    /// Upload CSV bytes.
    pub async fn upload_csv(&self, key: &str, data: Vec<u8>) -> Result<String> {
        self.upload(key, data, "text/csv").await
    }

    /// Upload HTML bytes.
    pub async fn upload_html(&self, key: &str, data: Vec<u8>) -> Result<String> {
        self.upload(key, data, "text/html").await
    }

    /// Download bytes from S3.
    pub async fn download(&self, key: &str) -> Result<Vec<u8>> {
        let resp = self.client
            .get_object()
            .bucket(&self.bucket)
            .key(key)
            .send()
            .await?;
        let bytes = resp.body.collect().await?.into_bytes().to_vec();
        Ok(bytes)
    }

    /// Build the S3 key for a report CSV.
    /// Format: reports/{tenant_id}/{report_type}/{name}.csv
    pub fn report_csv_key(tenant_id: i64, report_type: i32, name: &str) -> String {
        let safe_name = name.replace(['/', '\\', ' '], "_");
        format!("reports/{}/{}/{}.csv", tenant_id, report_type, safe_name)
    }

    /// Build the S3 key for an account report HTML.
    pub fn account_report_html_key(tenant_id: i64, account_id: i64, date: &str) -> String {
        format!("account-reports/{}/{}/{}.html", tenant_id, account_id, date)
    }
}
