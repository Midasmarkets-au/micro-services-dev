# 颜色系统说明

## 概述

项目中所有的颜色都通过 CSS 变量管理，只需要在 `src/app/globals.css` 中修改即可全局生效。

## 状态颜色

### 错误颜色（Error）

**日间模式：**
- 文字颜色：`#800020` (酒红色，与品牌色一致)
- 背景颜色：`rgba(128, 0, 32, 0.1)` (半透明)
- 边框颜色：`#800020`

**夜间模式：**
- 文字颜色：`#ff4d4f` (红色)
- 背景颜色：`rgba(255, 77, 79, 0.2)` (半透明)
- 边框颜色：`#ff4d4f`

### 成功颜色（Success）

**日间模式：**
- 文字颜色：`#10b981` (绿色)
- 背景颜色：`rgba(16, 185, 129, 0.1)` (半透明)

**夜间模式：**
- 文字颜色：`#52c41a` (绿色)
- 背景颜色：`rgba(82, 196, 26, 0.2)` (半透明)

## CSS 变量定义

在 `globals.css` 中定义：

```css
/* 日间模式 */
:root {
  /* 状态颜色 */
  --color-error: #800020;
  --color-error-bg: rgba(128, 0, 32, 0.1);
  --color-error-border: #800020;
  --color-success: #10b981;
  --color-success-bg: rgba(16, 185, 129, 0.1);
}

/* 夜间模式 */
.dark {
  /* 夜间模式状态颜色 */
  --color-error: #ff4d4f;
  --color-error-bg: rgba(255, 77, 79, 0.2);
  --color-error-border: #ff4d4f;
  --color-success: #52c41a;
  --color-success-bg: rgba(82, 196, 26, 0.2);
}
```

## 预定义 CSS 类

### 状态样式

```css
/* 错误文字 */
.error-text {
  color: var(--color-error);
}

/* 错误边框 */
.error-border {
  border-color: var(--color-error-border);
}

/* 错误提示横幅 */
.error-banner {
  background-color: var(--color-error-bg);
  color: var(--color-error);
  padding: 1rem;
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
}
```

### 实用工具类

项目已为所有 CSS 变量提供了简化的工具类，可以直接使用：

#### 背景颜色类

```css
.bg-background          /* 页面背景色 */
.bg-surface             /* 卡片/面板背景色 */
.bg-surface-secondary   /* 次要表面背景色 */
.bg-input-bg            /* 输入框背景色 */
.bg-primary             /* 主色背景 */
.bg-primary-light       /* 主色浅背景 */
.bg-primary-hover       /* 主色悬停背景 */
```

#### 边框颜色类

```css
.border-border          /* 标准边框色 */
.border-divider         /* 分割线颜色 */
.border-primary         /* 主色边框 */
```

#### 文字颜色类

```css
.text-text-primary      /* 主要文字颜色 */
.text-text-secondary    /* 次要文字颜色 */
.text-text-placeholder  /* 占位符文字颜色 */
.text-text-link         /* 链接文字颜色 */
.text-text-highlight    /* 高亮文字颜色 */
.text-primary           /* 主色文字 */
```

#### 阴影类

```css
.shadow-card            /* 卡片阴影 */
.shadow-dropdown        /* 下拉菜单阴影 */
```

### 成功样式

```css
/* 成功提示横幅 */
.success-banner {
  background-color: var(--color-success-bg);
  color: var(--color-success);
  padding: 1rem;
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
}
```

## 使用示例

### 状态样式使用

#### 1. 错误文字

```tsx
<span className="error-text">错误信息</span>
```

#### 2. 错误边框

```tsx
<input className={`input-field ${error ? 'error-border' : ''}`} />
```

#### 3. 错误提示横幅

```tsx
{error && (
  <div className="error-banner animate-fade-in">
    {error}
  </div>
)}
```

### 实用工具类使用

#### 1. 使用简化类名（推荐 ✅）

```tsx
// 背景色
<div className="bg-surface border border-border rounded">
  内容
</div>

// 文字颜色
<p className="text-text-primary">主要文字</p>
<span className="text-text-secondary">次要文字</span>

// 组合使用
<div className="bg-input-bg border border-border text-text-primary">
  <input className="bg-transparent placeholder:text-text-placeholder" />
</div>

// 悬停效果
<button className="bg-surface hover:bg-surface-secondary border border-border">
  按钮
</button>
```

#### 2. 使用 CSS 变量（备选方案）

如果需要特殊场景（如条件样式、内联样式），可以使用原始 CSS 变量：

```tsx
// Tailwind 中使用
<div className="bg-[var(--color-surface)]">内容</div>

// 内联样式中使用
<div style={{ backgroundColor: 'var(--color-surface)' }}>内容</div>
```

#### 3. 实际组件示例

```tsx
// 下拉菜单
<div className="bg-surface border border-border shadow-dropdown rounded">
  <div className="border-b border-border p-2">
    <input 
      className="w-full bg-input-bg border border-border text-text-primary 
                 placeholder:text-text-placeholder focus:border-primary" 
      placeholder="搜索..."
    />
  </div>
  <div className="p-2">
    <button className="hover:bg-surface-secondary text-text-primary">
      选项 1
    </button>
  </div>
</div>

// 卡片
<div className="bg-surface shadow-card rounded p-6">
  <h3 className="text-text-primary mb-2">标题</h3>
  <p className="text-text-secondary">描述文字</p>
</div>
```

### 4. 成功提示横幅

```tsx
{success && (
  <div className="success-banner animate-fade-in">
    {success}
  </div>
)}
```

## 组件中的应用

### Input 组件

```tsx
// 错误文字显示在 label 右侧
{error && <span className="error-text text-sm font-normal">{error}</span>}

// 错误时输入框显示红色边框
className={`input-field ${error ? 'error-border' : ''}`}
```

### Select 组件

```tsx
// 与 Input 组件相同
{error && <span className="error-text text-sm font-normal">{error}</span>}
className={`input-field ${error ? 'error-border' : ''}`}
```

### 表单页面

```tsx
// 错误提示
{errors.root && (
  <div className="error-banner animate-fade-in">
    {errors.root.message}
  </div>
)}

// 成功提示
{success && (
  <div className="success-banner animate-fade-in">
    {success}
  </div>
)}
```

## 为什么使用实用工具类

### 优势

1. **简洁易读** ✅
   ```tsx
   // 之前：冗长的 CSS 变量写法
   <div className="bg-[var(--color-surface)] border-[var(--color-border)]">
   
   // 现在：简洁的工具类
   <div className="bg-surface border-border">
   ```

2. **易于维护** 🔧
   - 所有颜色集中在 `globals.css` 管理
   - 修改一次，全局生效
   - 避免重复的长串 CSS 变量名

3. **一致性** 🎨
   - 团队成员使用统一的类名
   - 减少命名不一致的问题
   - 便于代码审查

4. **开发效率** ⚡
   - 自动补全更友好
   - 减少拼写错误
   - 代码更短更清晰

### 对比示例

```tsx
// ❌ 旧方式：使用 CSS 变量（冗长）
<div className="bg-[var(--color-input-bg)] border-[var(--color-border)] text-[var(--color-text-primary)]">
  <span className="text-[var(--color-text-secondary)]">...</span>
</div>

// ✅ 新方式：使用工具类（简洁）
<div className="bg-input-bg border-border text-text-primary">
  <span className="text-text-secondary">...</span>
</div>
```

## 如何修改颜色

### 修改错误颜色

只需在 `src/app/globals.css` 中修改：

```css
:root {
  --color-error: #800020;        /* 改成你想要的颜色 */
  --color-error-bg: rgba(128, 0, 32, 0.1);
  --color-error-border: #800020;
}

.dark {
  --color-error: #ff4d4f;        /* 夜间模式的颜色 */
  --color-error-bg: rgba(255, 77, 79, 0.2);
  --color-error-border: #ff4d4f;
}
```

修改后，所有使用 `.error-text`、`.error-border`、`.error-banner` 的地方都会自动更新！

### 修改其他颜色（背景、边框、文字等）

同样的道理，修改任何 CSS 变量都会影响对应的工具类：

```css
:root {
  /* 修改输入框背景色 */
  --color-input-bg: #f8f8f8;  /* 所有 .bg-input-bg 都会更新 */
  
  /* 修改边框颜色 */
  --color-border: #eeeeee;    /* 所有 .border-border 都会更新 */
  
  /* 修改主要文字颜色 */
  --color-text-primary: #333333;  /* 所有 .text-text-primary 都会更新 */
}
```

**重点：** 只需要修改一次 CSS 变量，项目中所有使用对应工具类的地方都会自动更新！🎉

### 修改成功颜色

```css
:root {
  --color-success: #10b981;      /* 改成你想要的颜色 */
  --color-success-bg: rgba(16, 185, 129, 0.1);
}

.dark {
  --color-success: #52c41a;
  --color-success-bg: rgba(82, 196, 26, 0.2);
}
```

## 优势

✅ **集中管理**：所有颜色在一个文件中定义
✅ **易于修改**：只需修改 CSS 变量即可全局生效
✅ **主题支持**：自动适配日间/夜间模式
✅ **类型安全**：使用预定义的 CSS 类，避免拼写错误
✅ **一致性**：确保整个应用使用统一的颜色规范

## 注意事项

1. 不要在代码中直接使用 `text-red-500`、`border-red-500` 等 Tailwind 类
2. 使用预定义的 CSS 类：`error-text`、`error-border`、`error-banner`
3. 如需新增状态颜色，在 `globals.css` 中添加 CSS 变量和对应的类
4. 修改颜色时记得同时更新日间和夜间模式
