# MDM Management System - Frontend

基于 Next.js 16 + React 19 + TypeScript + Tailwind CSS 4 构建的管理系统前端。

## 特性

- ⚡ **Turbopack** - 使用 Turbopack 加速开发
- 🌍 **国际化 (i18n)** - 支持 11 种语言 (next-intl)
- 🔐 **RBAC 权限控制** - 基于角色的访问控制
- 🍪 **JWT + HttpOnly Cookie** - 安全的认证方案
- 🌙 **深色/浅色主题** - 支持日间模式和夜间模式切换
- 📝 **表单验证** - React Hook Form + Zod
- 🗃️ **状态管理** - Zustand（支持持久化）
- 🎨 **骨架屏加载** - 流畅的加载体验
- 🔗 **.NET 后端集成** - 与 Midas Markets API 无缝对接

## 技术栈

| 技术 | 版本 | 说明 |
|------|------|------|
| Next.js | 16.0.8 | React 框架 |
| React | 19.2.1 | UI 库 |
| TypeScript | 5.x | 类型安全 |
| Tailwind CSS | 4.x | 样式框架 |
| Zustand | 5.x | 状态管理 |
| React Hook Form | 7.x | 表单处理 |
| Zod | 4.x | 数据验证 |
| next-intl | 4.x | 国际化 |
| Radix UI | - | 无障碍组件库 |

## 设计系统

### 主题颜色

| 变量 | 日间模式 | 夜间模式 |
|------|---------|---------|
| `--color-primary` | #800020 | #800020 |
| `--color-background` | #f8f8f8 | #0a0a0a |
| `--color-surface` | #ffffff | #1a1a1a |
| `--color-text-primary` | #333333 | #ffffff |
| `--color-text-secondary` | #999999 | #999999 |

### UI 组件

```
src/components/ui/
├── Button.tsx          # 按钮组件
├── Input.tsx           # 输入框组件
├── Select.tsx          # 选择器组件
├── SearchableSelect.tsx # 可搜索选择器
├── SelectInput.tsx     # 通用组合选择输入（下拉+输入框）
├── Logo.tsx            # 品牌 Logo
├── ThemeToggle.tsx     # 主题切换
├── LanguageToggle.tsx  # 语言切换
├── Toast.tsx           # 消息提示
├── ToastProvider.tsx   # Toast 上下文
├── PageLoading.tsx     # MDM 品牌加载动画
├── Skeleton.tsx        # 骨架屏组件
└── radix/              # Radix UI 封装组件
    ├── Button.tsx
    ├── Checkbox.tsx
    ├── DatePicker.tsx
    ├── Input.tsx
    ├── Select.tsx
    └── TimePicker.tsx
```

### 布局组件

```
src/components/layout/
├── AuthHeader.tsx          # 认证页面头部
├── AuthFooter.tsx          # 认证页面底部
├── AuthIllustration.tsx    # 认证页面插图
├── DashboardHeader.tsx     # 仪表盘头部导航
├── DashboardSidebar.tsx    # 仪表盘侧边栏
├── DashboardMainContent.tsx # 仪表盘主内容区
└── DashboardNotifications.tsx # 仪表盘通知栏
```

### 验证组件

```
src/components/verification/
├── VerificationBanner.tsx   # 验证页面 Banner
├── VerificationStepper.tsx  # 步骤指示器
├── PersonalInfoForm.tsx     # 个人信息表单
├── FinancialInfoForm.tsx    # 财务信息表单
├── AgreementForm.tsx        # 协议签署表单
├── DocumentUpload.tsx       # 文档上传组件
└── VerificationComplete.tsx # 验证完成页面
```

## 项目结构

```
src/
├── app/                    # Next.js App Router
│   ├── (auth)/            # 认证相关页面（公开）
│   │   ├── sign-in/       # 登录
│   │   ├── sign-up/       # 注册
│   │   ├── forgot-password/ # 忘记密码
│   │   ├── reset-password/  # 重置密码
│   │   ├── set-token/     # Token 登录
│   │   └── lead-create/   # Lead 创建
│   ├── (protected)/       # 需要认证的页面
│   │   ├── dashboard/     # 仪表盘
│   │   └── verification/  # 身份验证
│   └── api/               # API Routes
│       ├── auth/          # 认证相关
│       ├── user/          # 用户相关
│       ├── notifications/ # 通知
│       ├── configuration/ # 站点配置
│       └── verification/  # 验证相关
├── components/            # React 组件
│   ├── layout/           # 布局组件
│   ├── ui/               # UI 组件
│   ├── providers/        # Context Providers
│   └── verification/     # 验证流程组件
├── hooks/                 # 自定义 Hooks
│   ├── useApiClient.ts   # API 请求封装
│   ├── useTheme.ts       # 主题 Hook
│   ├── useToast.ts       # Toast Hook
│   ├── useLogout.ts      # 登出 Hook
│   └── useUserData.ts    # 用户数据 Hook
├── stores/                # Zustand 状态管理
│   └── userStore.ts      # 用户状态
├── lib/                   # 核心库
│   ├── api/              # API 客户端
│   ├── auth/             # 认证工具
│   └── rbac/             # 权限控制
├── i18n/                  # 国际化配置
├── messages/              # 多语言文件（11种语言）
└── types/                 # TypeScript 类型定义
```

## 路由

### 公开路由

| 路由 | 描述 |
|------|------|
| `/sign-in` | 登录页面 |
| `/sign-up` | 注册页面 |
| `/forgot-password` | 忘记密码 |
| `/reset-password` | 重置密码（邮件链接） |
| `/set-token` | Token 登录（后端生成链接） |
| `/lead-create` | Lead 创建 |

### 受保护路由

| 路由 | 描述 |
|------|------|
| `/dashboard` | 仪表盘 |
| `/verification` | 身份验证流程 |

## API Routes

### 认证相关

| 路由 | 方法 | 描述 |
|------|------|------|
| `/api/auth/login` | POST | 用户登录 |
| `/api/auth/register` | POST | 用户注册 |
| `/api/auth/logout` | POST | 用户登出 |
| `/api/auth/me` | GET | 获取当前用户 |
| `/api/auth/forgot-password` | POST | 发送重置密码邮件 |
| `/api/auth/password-reset` | POST | 重置密码 |
| `/api/auth/set-token` | POST | 设置 Token |
| `/api/auth/resend-confirmation` | POST | 重发确认邮件 |

### 业务相关

| 路由 | 方法 | 描述 |
|------|------|------|
| `/api/user/me` | GET | 获取用户详细信息 |
| `/api/configuration` | GET | 获取站点配置 |
| `/api/notifications` | GET | 获取通知列表 |
| `/api/verification` | GET | 获取验证状态 |
| `/api/verification/info` | POST | 提交个人信息 |
| `/api/verification/financial` | POST | 提交财务信息 |
| `/api/verification/agreement` | POST | 提交协议 |
| `/api/verification/document` | POST | 提交文档 |
| `/api/verification/upload` | POST | 上传文件 |

## 国际化

支持 11 种语言：

| 语言 | 代码 |
|------|------|
| 英语 | en |
| 简体中文 | zh |
| 繁体中文 | zh-tw |
| 日语 | jp |
| 韩语 | ko |
| 西班牙语 | es |
| 越南语 | vi |
| 泰语 | th |
| 马来语 | ms |
| 印尼语 | id |
| 柬埔寨语 | km |

```tsx
import { useTranslations } from 'next-intl';

function MyComponent() {
  const t = useTranslations('auth');
  return <button>{t('login')}</button>;
}
```

## 开发

```bash
# 安装依赖
npm install

# 启动开发服务器 (使用 Turbopack)
npm run dev

# 构建生产版本
npm run build

# 启动生产服务器
npm run start

# 代码检查
npm run lint
```

## 环境变量

创建 `.env.local` 文件：

```env
# API 配置
NEXT_PUBLIC_API_BASE_URL=https://midasmarkets.net.au
NEXT_PUBLIC_API_VERSION=v1
```

## 主题切换

项目支持日间模式和夜间模式，通过点击头部的主题切换按钮可以切换。主题设置会保存在 localStorage 中。

```tsx
import { ThemeToggle } from '@/components/ui';
import { useTheme } from '@/hooks/useTheme';

// 使用组件
<ThemeToggle />

// 使用 Hook
const { isDark, toggleTheme, mounted } = useTheme();
```

## 状态管理

使用 Zustand 管理全局状态，支持 localStorage 持久化：

```tsx
import { useUserStore } from '@/stores/userStore';

// 获取用户信息
const user = useUserStore((state) => state.user);

// 检查角色
const isIB = useUserStore((state) => state.isIB());

// 清除状态
const clearStore = useUserStore((state) => state.clearStore);
```

## API 请求

使用 `useApiClient` Hook 进行 API 请求：

```tsx
import { useApiClient } from '@/hooks/useApiClient';

function MyComponent() {
  const { get, post, upload } = useApiClient();

  // GET 请求
  const result = await get<DataType>('/api/endpoint');

  // POST 请求
  const result = await post('/api/endpoint', { data });

  // 文件上传
  const result = await upload('/api/upload', formData);
}
```

## License

MIT
