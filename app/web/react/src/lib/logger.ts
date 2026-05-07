/**
 * 服务端 logger：
 *   - production：单行 JSON 输出，兼容 Loki/fluent-bit 采集
 *   - development：原生 console，保留多行格式便于本地调试
 * 客户端始终降级为原生 console。
 */

type LogLevel = 'info' | 'warn' | 'error' | 'debug';

const isServer = typeof window === 'undefined';
const isProd = process.env.NODE_ENV === 'production';

function log(level: LogLevel, message: string, data?: unknown): void {
  if (isServer && isProd) {
    const entry: Record<string, unknown> = {
      ts: new Date().toISOString(),
      level,
      msg: message,
    };
    if (data !== undefined) {
      entry.data = data;
    }
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
