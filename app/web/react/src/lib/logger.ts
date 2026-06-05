/**
 * 服务端 logger：
 *   - production：单行 JSON 输出，兼容 Loki/fluent-bit 采集
 *   - development：原生 console，保留多行格式便于本地调试
 * 客户端始终降级为原生 console。
 *
 * 环境变量（仅服务端生效）：
 *   LOG_LEVEL=debug|info|warn|error   过滤日志级别（默认 debug，即全部输出）
 *   LOG_FORMAT=json|console           输出格式（默认 production=json，development=console）
 */

type LogLevel = 'info' | 'warn' | 'error' | 'debug';

const LEVEL_PRIORITY: Record<LogLevel, number> = { debug: 0, info: 1, warn: 2, error: 3 };
const envLevel = ((process.env.LOG_LEVEL ?? 'debug').toLowerCase()) as LogLevel;
const minPriority = LEVEL_PRIORITY[envLevel] ?? 0;

const isServer = typeof window === 'undefined';
const isProd = process.env.NODE_ENV === 'production';

// LOG_FORMAT 显式指定时优先，否则 production 默认 json，development 默认 console
const envFormat = process.env.LOG_FORMAT?.toLowerCase();
const useJson = isServer && (envFormat === 'json' || (!envFormat && isProd));

function serializeError(err: unknown): unknown {
  if (!(err instanceof Error)) return err;
  const obj: Record<string, unknown> = {
    name: err.name,
    message: err.message,
  };
  if (err.stack) obj.stack = err.stack;
  if (err.cause) obj.cause = serializeError(err.cause);
  for (const key of Object.getOwnPropertyNames(err)) {
    if (!(key in obj)) obj[key] = (err as unknown as Record<string, unknown>)[key];
  }
  return obj;
}

function log(level: LogLevel, message: string, data?: unknown): void {
  if (LEVEL_PRIORITY[level] < minPriority) return;

  if (useJson) {
    const entry: Record<string, unknown> = {
      ts: new Date().toISOString(),
      level,
      msg: message,
    };
    if (data !== undefined) entry.data = serializeError(data);
    const line = JSON.stringify(entry);
    if (level === 'error') {
      process.stderr.write(line + '\n');
    } else {
      process.stdout.write(line + '\n');
    }
  } else {
    const fn = level === 'error' ? console.error : level === 'warn' ? console.warn : console.log;
    data !== undefined ? fn(message, data) : fn(message);
  }
}

const logger = {
  info:  (message: string, data?: unknown) => log('info',  message, data),
  warn:  (message: string, data?: unknown) => log('warn',  message, data),
  error: (message: string, data?: unknown) => log('error', message, data),
  debug: (message: string, data?: unknown) => log('debug', message, data),
};

export default logger;
