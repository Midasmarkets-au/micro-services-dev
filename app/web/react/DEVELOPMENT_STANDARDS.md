# 开发规范文档 (Development Standards)

本文档记录了 MM-Front-Web 项目的所有开发规范和标准，确保代码的一致性和可维护性。

---

## 📱 响应式设计规范

### 1. 断点标准
使用 Tailwind CSS 默认断点：
- `sm`: 640px - 手机横屏、小平板
- `md`: 768px - 平板
- `lg`: 1024px - 小桌面
- `xl`: 1280px - 桌面
- `2xl`: 1536px - 大桌面

### 2. 移动优先 (Mobile First)
- **始终**使用移动优先的响应式设计方法
- 默认样式适用于移动设备（< 640px）
- 使用断点前缀向上适配更大屏幕

```tsx
// ✅ 正确示例 - 移动优先
<div className="w-full md:w-1/2 lg:w-1/3">

// ❌ 错误示例 - 桌面优先
<div className="w-1/3 lg:w-1/2 md:w-full">
```

### 3. 适配要求
- 所有页面必须支持 **320px 到 1920px** 的屏幕宽度
- 文本、图片、布局必须在不同尺寸下都可读可用
- 触摸目标（按钮、链接）在移动端至少 **44px × 44px**

---

## 🎨 样式规范

### 1. 单位标准
- **间距/尺寸**：优先使用 `rem` 单位
  ```tsx
  // ✅ 推荐
  className="mb-[2rem] p-[1.5rem] gap-[1rem]"
  
  // ❌ 避免
  className="mb-8 p-6 gap-4"  // Tailwind 默认单位（除非特殊情况）
  ```

- **字体大小**：使用 `rem` 或 Tailwind 的 text-* 类
  ```tsx
  className="text-[1.5rem]"  // 或 text-2xl
  ```

### 2. 颜色系统
使用设计 token，定义在 `tailwind.config.ts` 中：

```typescript
// 主题颜色
bg-primary          // 主色背景
text-primary        // 主色文字
bg-surface-*        // 表面背景色
text-text-*         // 文字颜色
border-border-*     // 边框颜色
```

- **必须**支持亮色/暗色主题
- 使用 `dark:` 前缀定义暗色样式

```tsx
className="bg-white dark:bg-zinc-900 text-zinc-900 dark:text-white"
```

### 3. 通用样式类
项目中定义的全局样式类（在 `globals.css` 中）：

```css
.card              // 卡片容器
.btn-primary       // 主要按钮
.btn-secondary     // 次要按钮
.input-field       // 输入框
.input-label       // 输入框标签
.divider           // 分割线
```

使用这些类保持一致性！

---

## 🌍 国际化规范

### 1. 文本国际化要求
- **所有**用户可见的文本必须国际化
- 使用 `next-intl` 的 `useTranslations` hook

```tsx
// ✅ 正确
const t = useTranslations('auth');
<button>{t('login')}</button>

// ❌ 错误
<button>登录</button>
```

### 2. 翻译文件组织
- 中文：`src/messages/zh.json`
- 英文：`src/messages/en.json`

按功能模块组织：
```json
{
  "common": { ... },    // 通用文本
  "auth": { ... },      // 认证相关
  "dashboard": { ... }, // 仪表盘
  "nav": { ... }        // 导航
}
```

### 3. 表单验证消息
使用 `Input` 组件的 `requiredMessage` 和 `invalidMessage` props：

```tsx
<Input
  required
  requiredMessage={t('emailRequired')}
  invalidMessage={t('emailInvalid')}
/>
```

---

## ⚛️ React 组件规范

### 1. 组件结构
```tsx
'use client';  // 客户端组件标记（如需要）

import { ... } from 'react';
import { useTranslations } from 'next-intl';  // 如需要

interface ComponentProps {
  // Props 定义
}

export function ComponentName({ ...props }: ComponentProps) {
  const t = useTranslations('namespace');  // 如需要
  
  // Hooks
  // State
  // Effects
  // Handlers
  
  return (
    // JSX
  );
}
```

### 2. 客户端 vs 服务端组件
- **默认**使用服务端组件（无 `'use client'`）
- 需要以下功能时使用客户端组件：
  - State (`useState`, `useReducer`)
  - Effects (`useEffect`, `useLayoutEffect`)
  - 事件处理器 (`onClick`, `onChange` 等)
  - 浏览器 API (`localStorage`, `window` 等)
  - Context

### 3. 组件文件位置
```
src/
  components/
    ui/           // 通用 UI 组件（Button, Input, etc.）
    layout/       // 布局组件（Header, Footer, Sidebar）
    features/     // 功能特定组件
  app/
    (auth)/       // 认证相关页面
    (protected)/  // 需要认证的页面
    api/          // API 路由
```

---

## 🎯 主题切换规范

### 1. 避免闪烁
- 在 `layout.tsx` 的 `<head>` 中添加内联脚本
- 脚本在渲染前读取 localStorage 并设置主题

```tsx
<script dangerouslySetInnerHTML={{
  __html: `
    (function() {
      const theme = localStorage.getItem('theme');
      const isDark = theme === 'dark' || 
        (!theme && window.matchMedia('(prefers-color-scheme: dark)').matches);
      if (isDark) {
        document.documentElement.classList.add('dark');
      }
    })();
  `
}} />
```

### 2. 服务端/客户端一致性
- 使用 `mounted` 状态避免 hydration 不匹配
- 初始状态在服务端和客户端保持一致

```tsx
const [mounted, setMounted] = useState(false);

useEffect(() => {
  setMounted(true);
}, []);

if (!mounted) {
  return <PlaceholderComponent />;
}
```

---

## 📝 表单处理规范

### 1. 受控组件
始终使用受控组件：

```tsx
const [formData, setFormData] = useState({ email: '', password: '' });

<Input
  value={formData.email}
  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
/>
```

### 2. 错误处理
```tsx
const [error, setError] = useState('');

try {
  // API 调用
} catch (err) {
  setError(t('errorMessage'));
}

{error && (
  <div className="text-red-600 dark:text-red-400">
    {error}
  </div>
)}
```

### 3. 加载状态
```tsx
const [loading, setLoading] = useState(false);

<Button type="submit" loading={loading}>
  {t('submit')}
</Button>
```

---

## 🔒 类型安全规范

### 1. TypeScript 严格模式
- 启用 `strict: true`
- 所有函数参数和返回值必须有类型标注
- 避免使用 `any`，使用 `unknown` 或具体类型

```tsx
// ✅ 正确
interface User {
  id: string;
  email: string;
  role: Role;
}

function getUser(id: string): Promise<User> {
  // ...
}

// ❌ 错误
function getUser(id): any {
  // ...
}
```

### 2. Props 类型定义
```tsx
interface ButtonProps {
  variant?: 'primary' | 'secondary' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
  children: ReactNode;
  onClick?: () => void;
}
```

---

## 🔐 认证和权限规范

### 1. 路由保护
- 认证页面：`app/(auth)/` 路由组
- 受保护页面：`app/(protected)/` 路由组
- 在 `middleware.ts` 中统一处理路由保护

### 2. 权限检查
使用 RBAC 组件：

```tsx
import { PermissionGate } from '@/lib/rbac';

<PermissionGate user={user} permission="user:create">
  <AdminOnlyFeature />
</PermissionGate>
```

---

## 🚀 性能规范

### 1. 图片优化
使用 Next.js `Image` 组件：

```tsx
import Image from 'next/image';

<Image
  src="/images/logo.svg"
  alt="Logo"
  width={40}
  height={40}
  priority  // 首屏图片
/>
```

### 2. 代码分割
- 大型组件使用动态导入
```tsx
const HeavyComponent = dynamic(() => import('./HeavyComponent'), {
  loading: () => <Spinner />
});
```

---

## 📦 导入规范

### 1. 导入顺序
```tsx
// 1. React 核心
import { useState, useEffect } from 'react';

// 2. Next.js
import { useRouter } from 'next/navigation';
import Image from 'next/image';

// 3. 第三方库
import { useTranslations } from 'next-intl';

// 4. 项目内部 - 绝对路径
import { Button, Input } from '@/components/ui';
import { getCurrentUser } from '@/lib/auth/session';
import type { User } from '@/types/auth';

// 5. 相对路径（避免使用）
import { helper } from './utils';
```

### 2. 类型导入
使用 `type` 关键字：

```tsx
import type { User, Role } from '@/types/auth';
```

---

## ✅ 代码质量规范

### 1. 命名约定
- 组件：`PascalCase` - `UserProfile`, `AuthLayout`
- 函数：`camelCase` - `handleSubmit`, `getUserData`
- 常量：`UPPER_SNAKE_CASE` - `API_BASE_URL`
- 文件名：
  - 组件：`PascalCase.tsx` - `Button.tsx`
  - 工具函数：`camelCase.ts` - `formatDate.ts`

### 2. 注释规范
```tsx
// ✅ 好的注释 - 解释为什么，不是什么
// 使用 setTimeout 避免 React 18 的并发模式警告
queueMicrotask(() => setMounted(true));

// ❌ 不好的注释 - 只描述代码做了什么
// 设置 mounted 为 true
setMounted(true);
```

---

## 🧪 测试规范（待补充）

_TODO: 添加测试规范_

---

## 📌 检查清单

在提交代码前，确保：

- [ ] 响应式设计：在手机、平板、桌面上都测试过
- [ ] 主题切换：亮色/暗色模式都正常显示
- [ ] 国际化：所有文本都已翻译（中英文）
- [ ] 类型安全：没有 TypeScript 错误
- [ ] 无 linter 错误
- [ ] 无主题闪烁或 hydration 警告
- [ ] 使用 `rem` 单位而不是 `px`
- [ ] 代码格式符合项目规范

---

## 🔄 更新日志

| 日期 | 更新内容 | 更新人 |
|------|---------|--------|
| 2025-12-09 | 初始创建规范文档 | - |

---

**注意**: 本文档是活的文档，随着项目发展会持续更新。如有新的规范或标准，请及时更新此文档。
