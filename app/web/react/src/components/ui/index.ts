// ===== 统一组件导出入口 =====
// 所有组件都从这里导出，项目中统一使用 '@/components/ui' 导入
// 组件实现在 ./radix 文件夹中，如需修改路径只需更新此文件

// Input & Textarea
export { Input, Textarea } from './radix/Input';
export type { InputProps, InputSize, TextareaProps } from './radix/Input';

// Button
export { Button, buttonVariants } from './radix/Button';
export type { ButtonProps } from './radix/Button';

// Toast
export { Toast } from './radix/Toast';
export type { ToastType, ToastProps } from './radix/Toast';

// Select (Radix UI)
export {
  Select,
  SelectGroup,
  SelectValue,
  SelectTrigger,
  SelectContent,
  SelectLabel,
  SelectItem,
  SelectSeparator,
  SimpleSelect,
} from './radix/Select';
export type { SelectOption, SimpleSelectProps } from './radix/Select';

// DatePicker & TimePicker
export { DatePicker, formatDateForApi } from './radix/DatePicker';
export type { DatePickerProps, DateRange } from './radix/DatePicker';
export { TimePicker } from './radix/TimePicker';

// Checkbox
export { Checkbox } from './radix/Checkbox';

// ===== 其他 UI 组件 =====

// Avatar
export { Avatar } from './Avatar';
export type { AvatarProps } from './Avatar';

// Logo
export { Logo } from './Logo';

// Theme & Language Toggle
export { ThemeToggle } from './ThemeToggle';
export { LanguageToggle } from './LanguageToggle';

// SearchableSelect (基于 react-select)
export { SearchableSelect } from './SearchableSelect';
export { SearchFilter } from './SearchFilter';
export type { SearchFilterResult } from './SearchFilter';

// SelectInput (通用组合选择输入框)
export { SelectInput, PhoneInput } from './SelectInput';

// ToastProvider
export { ToastProvider, useToastContext } from './ToastProvider';
export type { ToastOptions } from './ToastProvider';

// AuthResultState
export { AuthResultState, AuthSuccessState, AuthErrorState } from './AuthResultState';

// Skeleton
export { 
  Skeleton, 
  SidebarSkeleton, 
  MainContentSkeleton, 
  NotificationsSkeleton,
  DashboardSkeleton 
} from './Skeleton';

// PageLoading
export { PageLoading, MiniLoading, MDMLoading } from './PageLoading';

// FileCard
export { FileCard } from './FileCard';

// FilePreviewModal
export { FilePreviewModal } from './FilePreviewModal';

// UploadFileModal
export { UploadFileModal, VerificationDocumentTypes } from './UploadFileModal';

// Dialog
export {
  Dialog,
  DialogPortal,
  DialogOverlay,
  DialogClose,
  DialogTrigger,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
} from './radix/Dialog';

// ConfirmDialog
export { ConfirmDialog } from './ConfirmDialog';

// Stepper
export { Stepper } from './Stepper';
export type { StepItem, StepStatus } from './Stepper';

// BalanceShow
export { BalanceShow, formatBalance, CurrencyCodeMap } from './BalanceShow';
export type { BalanceShowProps } from './BalanceShow';

// TransactionInfo & TransactionIcon
export { TransactionInfo, getAmountSign } from './TransactionInfo';
export type { TransactionInfoProps } from './TransactionInfo';
export { TransactionIcon } from './TransactionIcon';

// DateDisplay
export { DateDisplay, formatDateValue } from './DateDisplay';
export type { DateDisplayProps, DateTimezone, DateFormat } from './DateDisplay';

// EmptyState
export { EmptyState } from './EmptyState';
export type { EmptyStateProps } from './EmptyState';

// Tag
export { Tag } from './Tag';
export type { TagVariant, TagProps } from './Tag';

// Pagination
export { Pagination } from './Pagination';
export type { PaginationProps } from './Pagination';

// DataTable
export { DataTable } from './DataTable';
export type { DataTableColumn, DataTableProps, DataTableGroupConfig } from './DataTable';

export { DropdownMenu } from './DropdownMenu';
export type { DropdownMenuItem, DropdownMenuProps } from './DropdownMenu';

// Icon
export { Icon } from './Icon';
export type { IconProps } from './Icon';

// Switch
export { Switch } from './Switch';
export type { SwitchProps } from './Switch';

// Tabs
export { Tabs } from './Tabs';
export type { TabsProps, TabItem } from './Tabs';
