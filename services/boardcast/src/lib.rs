//! Boardcast Service — SSE 消息推送 + gRPC 广播接口
//!
//! 核心数据结构：`BroadcastBus`，以频道名为 key，`tokio::sync::broadcast::Sender` 为 value。
//! gRPC `Publish` 与 SSE 订阅共享同一个 bus，实现解耦的发布/订阅模型。

pub mod api {
    pub mod v1 {
        include!("generated/api.v1.rs");
    }
}

pub use api::v1::*;

use dashmap::DashMap;
use std::sync::Arc;
use tokio::sync::broadcast;

/// 每个频道的广播容量（消息队列深度）。
const CHANNEL_CAPACITY: usize = 1024;

/// 全局广播总线：频道名 → broadcast Sender。
///
/// 使用 `DashMap` 保证并发安全，无需外部锁。
#[derive(Clone)]
pub struct BroadcastBus {
    inner: Arc<DashMap<String, broadcast::Sender<String>>>,
}

impl BroadcastBus {
    pub fn new() -> Self {
        Self {
            inner: Arc::new(DashMap::new()),
        }
    }

    /// 获取或创建频道的 Sender。
    pub fn sender(&self, channel: &str) -> broadcast::Sender<String> {
        if let Some(tx) = self.inner.get(channel) {
            return tx.clone();
        }
        let (tx, _) = broadcast::channel(CHANNEL_CAPACITY);
        self.inner
            .entry(channel.to_string())
            .or_insert(tx)
            .clone()
    }

    /// 订阅指定频道，返回 Receiver。
    pub fn subscribe(&self, channel: &str) -> broadcast::Receiver<String> {
        self.sender(channel).subscribe()
    }

    /// 向指定频道发布消息，返回收到消息的订阅者数量。
    /// 若频道无订阅者，`send` 返回 Err，此处忽略（正常情况）。
    pub fn publish(&self, channel: &str, message: String) -> usize {
        let tx = self.sender(channel);
        tx.send(message).unwrap_or(0)
    }
}

impl Default for BroadcastBus {
    fn default() -> Self {
        Self::new()
    }
}
