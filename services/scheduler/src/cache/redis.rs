/// Redis cache — mirrors IMyCache used in ReportJob for AccountDailyConfirmation state coordination.
use anyhow::Result;
use redis::{aio::ConnectionManager, AsyncCommands, Client};
use std::time::Duration;

use crate::config::Config;

#[derive(Clone)]
pub struct RedisCache {
    conn: ConnectionManager,
}

impl RedisCache {
    pub async fn new(config: &Config) -> Result<Self> {
        let client = Client::open(config.redis_url.as_str())?;
        let conn = ConnectionManager::new(client).await?;
        Ok(Self { conn })
    }

    pub async fn get_string(&self, key: &str) -> Result<Option<String>> {
        let mut conn = self.conn.clone();
        let val: Option<String> = conn.get(key).await?;
        Ok(val)
    }

    pub async fn set_string(&self, key: &str, value: &str, ttl: Duration) -> Result<()> {
        let mut conn = self.conn.clone();
        let _: () = conn.set_ex(key, value, ttl.as_secs()).await?;
        Ok(())
    }

    pub async fn delete(&self, key: &str) -> Result<()> {
        let mut conn = self.conn.clone();
        let _: () = conn.del(key).await?;
        Ok(())
    }
}

// ── Key helpers (mirrors ReportJob static methods) ──────────────────────────

pub fn generate_account_task_key(tenant_id: i64, date: &str) -> String {
    format!("generate_account_task_tid:{}_date:{}", tenant_id, date)
}

pub fn process_account_report_task_key(tenant_id: i64, date: &str) -> String {
    format!("process_account_report_task_tid:{}_date:{}", tenant_id, date)
}

pub fn send_mail_task_key(tenant_id: i64, date: &str) -> String {
    format!("send_email_task_tid:{}_date:{}", tenant_id, date)
}

pub const TODAY_KEY: &str = "date_account_daily_report";
