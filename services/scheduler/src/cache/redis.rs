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
        let conn = client.get_connection_manager().await?;
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

    /// 从 Redis Hash 中获取字段值
    pub async fn hget(&self, key: &str, field: &str) -> Result<Option<String>> {
        let mut conn = self.conn.clone();
        let value: Option<String> = redis::cmd("HGET")
            .arg(key)
            .arg(field)
            .query_async(&mut conn)
            .await?;
        Ok(value)
    }

    /// 向 Redis Hash 中设置字段值
    pub async fn hset(&self, key: &str, field: &str, value: &str) -> Result<()> {
        let mut conn = self.conn.clone();
        let _: () = redis::cmd("HSET")
            .arg(key)
            .arg(field)
            .arg(value)
            .query_async(&mut conn)
            .await?;
        Ok(())
    }

    /// SISMEMBER key member — returns true if member is in the set.
    pub async fn sismember(&self, key: &str, member: &str) -> Result<bool> {
        let mut conn = self.conn.clone();
        let result: bool = redis::cmd("SISMEMBER")
            .arg(key)
            .arg(member)
            .query_async(&mut conn)
            .await?;
        Ok(result)
    }

    /// SADD key member + EXPIRE key ttl_secs — adds member to set and sets TTL.
    pub async fn sadd_with_ttl(&self, key: &str, member: &str, ttl: Duration) -> Result<()> {
        let mut conn = self.conn.clone();
        let _: () = redis::cmd("SADD")
            .arg(key)
            .arg(member)
            .query_async(&mut conn)
            .await?;
        let _: () = redis::cmd("EXPIRE")
            .arg(key)
            .arg(ttl.as_secs())
            .query_async(&mut conn)
            .await?;
        Ok(())
    }

    /// HINCRBY key field increment — increments a hash field by increment.
    pub async fn hincrby(&self, key: &str, field: &str, increment: i64) -> Result<i64> {
        let mut conn = self.conn.clone();
        let result: i64 = redis::cmd("HINCRBY")
            .arg(key)
            .arg(field)
            .arg(increment)
            .query_async(&mut conn)
            .await?;
        Ok(result)
    }

    /// HGET key field — returns the string value and tries to parse as i64.
    pub async fn hget_i64(&self, key: &str, field: &str) -> Result<Option<i64>> {
        let val = self.hget(key, field).await?;
        Ok(val.and_then(|s| s.parse::<i64>().ok()))
    }

    /// HSET key field value with TTL on the whole key (if not already set).
    pub async fn hset_with_ttl(&self, key: &str, field: &str, value: &str, ttl: Duration) -> Result<()> {
        let mut conn = self.conn.clone();
        let _: () = redis::cmd("HSET")
            .arg(key)
            .arg(field)
            .arg(value)
            .query_async(&mut conn)
            .await?;
        let _: () = redis::cmd("EXPIRE")
            .arg(key)
            .arg(ttl.as_secs())
            .arg("NX")
            .query_async(&mut conn)
            .await?;
        Ok(())
    }

    /// SET key value EX ttl NX — returns true if the lock was acquired.
    pub async fn set_nx(&self, key: &str, value: &str, ttl: Duration) -> Result<bool> {
        let mut conn = self.conn.clone();
        let result: Option<String> = redis::cmd("SET")
            .arg(key)
            .arg(value)
            .arg("EX")
            .arg(ttl.as_secs())
            .arg("NX")
            .query_async(&mut conn)
            .await?;
        Ok(result.is_some())
    }

    /// 删除整个 Redis key
    pub async fn del(&self, key: &str) -> Result<()> {
        let mut conn = self.conn.clone();
        let _: () = redis::cmd("DEL")
            .arg(key)
            .query_async(&mut conn)
            .await?;
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
