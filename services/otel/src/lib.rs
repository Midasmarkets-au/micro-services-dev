use tracing_appender::non_blocking::WorkerGuard;
use tracing_subscriber::{
    filter::LevelFilter,
    fmt,
    layer::{Layer, SubscriberExt},
    util::SubscriberInitExt,
    EnvFilter,
};

/// Holds the `WorkerGuard`s for the non-blocking file appenders.
/// Drop this only at program exit to ensure all buffered log records are flushed.
pub struct TracingGuard {
    _daily_guard: WorkerGuard,
    _error_guard: WorkerGuard,
}

/// Initialise the global tracing subscriber with:
/// - stdout pretty-format layer (filtered by `RUST_LOG`, default `info`)
/// - daily rolling file layer → `<log_dir>/<service_name>-.log` (JSON, all levels)
/// - daily rolling file layer → `<log_dir>/<service_name>-error-.log` (JSON, ERROR only)
///
/// `LOG_DIR` env var controls the log directory (default: `./logs`).
///
/// Returns a [`TracingGuard`] that **must be kept alive** for the duration of the
/// process so that the background log-writer threads are not dropped prematurely.
pub fn init_tracing(service_name: &'static str) -> TracingGuard {
    let log_dir = std::env::var("LOG_DIR").unwrap_or_else(|_| "./logs".to_string());

    // --- file appenders -------------------------------------------------
    let daily_appender =
        tracing_appender::rolling::daily(&log_dir, format!("{service_name}-.log"));
    let (daily_writer, daily_guard) = tracing_appender::non_blocking(daily_appender);

    let error_appender =
        tracing_appender::rolling::daily(&log_dir, format!("{service_name}-error-.log"));
    let (error_writer, error_guard) = tracing_appender::non_blocking(error_appender);

    // --- layers ---------------------------------------------------------
    let env_filter = EnvFilter::try_from_default_env()
        .unwrap_or_else(|_| EnvFilter::new("info"));

    let stdout_layer = fmt::layer().pretty();

    let daily_file_layer = fmt::layer()
        .json()
        .with_writer(daily_writer);

    let error_file_layer = fmt::layer()
        .json()
        .with_writer(error_writer)
        .with_filter(LevelFilter::ERROR);

    // --- registry -------------------------------------------------------
    tracing_subscriber::registry()
        .with(env_filter)
        .with(stdout_layer)
        .with(daily_file_layer)
        .with(error_file_layer)
        .init();

    TracingGuard {
        _daily_guard: daily_guard,
        _error_guard: error_guard,
    }
}
