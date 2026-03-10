import { SignJWT, jwtVerify } from 'jose';
import type { JWTPayload, Role, Permission } from '@/types/auth';

const JWT_SECRET = new TextEncoder().encode(
  process.env.JWT_SECRET || 'your-super-secret-key-change-in-production'
);

const JWT_EXPIRES_IN = process.env.JWT_EXPIRES_IN || '7d';

// 解析过期时间字符串为秒数
function parseExpiresIn(expiresIn: string): number {
  const match = expiresIn.match(/^(\d+)([smhd])$/);
  if (!match) return 7 * 24 * 60 * 60; // 默认7天

  const value = parseInt(match[1], 10);
  const unit = match[2];

  switch (unit) {
    case 's':
      return value;
    case 'm':
      return value * 60;
    case 'h':
      return value * 60 * 60;
    case 'd':
      return value * 24 * 60 * 60;
    default:
      return 7 * 24 * 60 * 60;
  }
}

// 生成 JWT Token
export async function signToken(payload: {
  userId: string;
  email: string;
  role: Role;
  permissions: Permission[];
}): Promise<string> {
  const expiresInSeconds = parseExpiresIn(JWT_EXPIRES_IN);

  return new SignJWT({
    userId: payload.userId,
    email: payload.email,
    role: payload.role,
    permissions: payload.permissions,
  })
    .setProtectedHeader({ alg: 'HS256' })
    .setIssuedAt()
    .setExpirationTime(`${expiresInSeconds}s`)
    .sign(JWT_SECRET);
}

// 验证 JWT Token
export async function verifyToken(token: string): Promise<JWTPayload | null> {
  try {
    const { payload } = await jwtVerify(token, JWT_SECRET);
    return payload as unknown as JWTPayload;
  } catch {
    return null;
  }
}

// 获取 Token 过期时间（毫秒）
export function getTokenExpiresMs(): number {
  return parseExpiresIn(JWT_EXPIRES_IN) * 1000;
}

