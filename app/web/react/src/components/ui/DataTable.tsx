'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import { Skeleton } from './Skeleton';
import { EmptyState } from './EmptyState';

/**
 * DataTable 列定义
 *
 * @example
 * const columns: DataTableColumn<User>[] = [
 *   { key: 'name', title: '客户', render: (item) => item.name, skeletonWidth: 'w-24' },
 *   { key: 'balance', title: '余额', render: (item) => <BalanceShow balance={item.balance} />, align: 'right' },
 * ];
 */
export interface DataTableColumn<T> {
  /** 唯一标识，用于 React key */
  key: string;
  /** 表头文字 */
  title: React.ReactNode;
  /** 单元格渲染函数 */
  render: (item: T, index: number) => React.ReactNode;
  /** 列宽 class，如 'w-[200px]' */
  width?: string;
  /** 对齐方式，默认 left */
  align?: 'left' | 'center' | 'right';
  /** 骨架屏占位宽度 class，默认 'w-20' */
  skeletonWidth?: string;
  /** 骨架屏占位高度 class，默认 'h-4' */
  skeletonHeight?: string;
  /** 自定义骨架屏渲染（复杂布局如头像+文字组合时使用） */
  skeletonRender?: () => React.ReactNode;
}

/**
 * 分组配置，用于按日期等字段将数据行分组显示
 *
 * 每个分组有独立的圆角边框，组内行用分隔线分开。
 * 分组头通过 renderGroupHeader 自定义渲染，会作为首列 rowSpan 显示。
 */
export interface DataTableGroupConfig<T> {
  /** 返回分组 key 的函数 */
  groupBy: (item: T) => string;
  /** 渲染分组头单元格内容（左侧日期区域） */
  renderGroupHeader: (groupKey: string, items: T[]) => React.ReactNode;
  /** 分组头单元格宽度 class */
  headerWidth?: string;
}

/**
 * DataTable 通用表格组件
 *
 * 两种样式模式：
 * 1. **默认模式** - thead 独立渲染，tbody 有外边框 + 行缩进分隔线
 * 2. **分组模式** (传入 groupConfig) - 无 tbody 外边框，按分组 key 将行分组，
 *    每组有独立圆角边框，分组头在左侧首列用 rowSpan 合并
 *
 * @example
 * // 默认模式
 * <DataTable columns={columns} data={users} rowKey={(item) => item.id} loading={isLoading} />
 *
 * // 分组模式（按日期分组）
 * <DataTable
 *   columns={columns}
 *   data={deposits}
 *   rowKey={(item) => item.id}
 *   groupConfig={{
 *     groupBy: (item) => new Date(item.createdOn).toDateString(),
 *     renderGroupHeader: (key) => <span>{key}</span>,
 *     headerWidth: 'w-[120px]',
 *   }}
 * />
 */
export interface DataTableProps<T> {
  /** 列定义 */
  columns: DataTableColumn<T>[];
  /** 数据数组 */
  data: T[];
  /** 行唯一 key 生成函数 */
  rowKey: (item: T, index: number) => string | number;
  /** 加载状态 */
  loading?: boolean;
  /** 骨架屏行数，默认 8 */
  skeletonRows?: number;
  /** 行点击事件 */
  onRowClick?: (item: T, index: number) => void;
  /** 当前高亮的行 key，匹配 rowKey 返回值时该行显示选中态背景 */
  activeRowKey?: string | number | null;
  /** 自定义空状态内容 */
  emptyContent?: React.ReactNode;
  /** 表格底部追加的自定义行（如合计行），仅在有数据且非 loading 时渲染 */
  footer?: React.ReactNode;
  /** 分组配置，启用后使用分组模式渲染 */
  groupConfig?: DataTableGroupConfig<T>;
  /** 外层容器 className */
  className?: string;
  /** 表格边框圆角大小，默认 'sm'。可选 'sm' | 'md' | 'lg' | 'xl' | 'none' */
  rounded?: 'none' | 'sm' | 'md' | 'lg' | 'xl';
  /** 是否拉伸表格高度填满父容器，默认 true */
  stretchHeight?: boolean;
}

const ALIGN_CLASS = {
  left: 'text-left',
  center: 'text-center',
  right: 'text-right',
};

const ROUNDED_MAP = {
  none: { tl: '', tr: '', bl: '', br: '' },
  sm: { tl: 'rounded-tl-sm', tr: 'rounded-tr-sm', bl: 'rounded-bl-sm', br: 'rounded-br-sm' },
  md: { tl: 'rounded-tl-md', tr: 'rounded-tr-md', bl: 'rounded-bl-md', br: 'rounded-br-md' },
  lg: { tl: 'rounded-tl-lg', tr: 'rounded-tr-lg', bl: 'rounded-bl-lg', br: 'rounded-br-lg' },
  xl: { tl: 'rounded-tl-xl', tr: 'rounded-tr-xl', bl: 'rounded-bl-xl', br: 'rounded-br-xl' },
} as const;

const TABLE_CLASS = 'w-full text-left text-sm whitespace-nowrap';

function groupData<T>(data: T[], groupBy: (item: T) => string): { key: string; items: T[] }[] {
  const map = new Map<string, T[]>();
  for (const item of data) {
    const k = groupBy(item);
    if (!map.has(k)) map.set(k, []);
    map.get(k)!.push(item);
  }
  return Array.from(map.entries()).map(([key, items]) => ({ key, items }));
}

export function DataTable<T>({
  columns,
  data,
  rowKey,
  loading = false,
  skeletonRows = 8,
  onRowClick,
  activeRowKey,
  emptyContent,
  footer,
  groupConfig,
  className,
  rounded = 'sm',
  stretchHeight = true,
}: DataTableProps<T>) {
  const colCount = columns.length;
  const r = ROUNDED_MAP[rounded];
  const singleRowAutoHeight = !loading && data.length === 1;
  const shouldStretch = stretchHeight && !singleRowAutoHeight;

  if (groupConfig) {
    return <GroupedDataTable columns={columns} data={data} rowKey={rowKey} loading={loading} skeletonRows={skeletonRows} onRowClick={onRowClick} activeRowKey={activeRowKey} emptyContent={emptyContent} footer={footer} groupConfig={groupConfig} className={className} />;
  }
  if (!loading && data.length === 0) {
    return (
      <div className={cn('flex flex-1 flex-col', className)}>
        <div className={cn('flex flex-1 items-center justify-center border border-border', rounded === 'none' ? '' : `rounded-${rounded}`)}>
          <div className="flex min-h-[300px] items-center justify-center">
            {emptyContent || <EmptyState />}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={cn('flex flex-col', className)}>
      <div
        className={cn(
          'flex flex-col overflow-x-auto',
          shouldStretch && 'flex-1'
        )}
      >
        <table
          className={cn(
            TABLE_CLASS,
            'border-separate border-spacing-0',
            shouldStretch && 'flex-1'
          )}
        >
          {/* Table Header — no border */}
          <thead>
            <tr className="text-sm text-text-secondary">
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={cn(
                    'px-5 py-3 font-medium',
                    col.width,
                    ALIGN_CLASS[col.align || 'left']
                  )}
                >
                  {col.title}
                </th>
              ))}
            </tr>
          </thead>

          {/* Table Body — bordered via td borders */}
          <tbody>
            {loading ? (
              Array.from({ length: skeletonRows }).map((_, rowIdx) => (
                <React.Fragment key={rowIdx}>
                  {rowIdx > 0 && (
                    <tr aria-hidden="true">
                      <td colSpan={colCount} className="border-l border-r border-border p-0">
                        <div className="mx-5 border-t border-border" />
                      </td>
                    </tr>
                  )}
                  <tr>
                    {columns.map((col, ci) => (
                      <td
                        key={col.key}
                        className={cn(
                          'px-5 py-4',
                          col.width,
                          ALIGN_CLASS[col.align || 'left'],
                          ci === 0 && 'border-l border-border',
                          ci === colCount - 1 && 'border-r border-border',
                          rowIdx === 0 && 'border-t border-border',
                          rowIdx === 0 && ci === 0 && r.tl,
                          rowIdx === 0 && ci === colCount - 1 && r.tr,
                          rowIdx === skeletonRows - 1 && 'border-b border-border',
                          rowIdx === skeletonRows - 1 && ci === 0 && r.bl,
                          rowIdx === skeletonRows - 1 && ci === colCount - 1 && r.br,
                        )}
                      >
                        {col.skeletonRender ? col.skeletonRender() : (
                          <Skeleton className={cn(col.skeletonHeight || 'h-4', col.skeletonWidth || 'w-20')} />
                        )}
                      </td>
                    ))}
                  </tr>
                </React.Fragment>
              ))
            ) : (
              data.map((item, idx) => {
                const isFirst = idx === 0;
                const isLast = idx === data.length - 1;
                const isActive =
                  activeRowKey != null && rowKey(item, idx) === activeRowKey;
                return (
                  <React.Fragment key={rowKey(item, idx)}>
                    {idx > 0 && (
                      <tr aria-hidden="true">
                        <td colSpan={colCount} className="border-l border-r border-border p-0">
                          <div className="mx-5 border-t border-border" />
                        </td>
                      </tr>
                    )}
                    <tr
                      className={cn(
                        'text-text-table transition-colors',
                        isActive
                          ? 'bg-surface-secondary'
                          : 'hover:bg-surface-secondary/50',
                        onRowClick && 'cursor-pointer'
                      )}
                      onClick={onRowClick ? () => onRowClick(item, idx) : undefined}
                    >
                      {columns.map((col, ci) => (
                        <td
                          key={col.key}
                          className={cn(
                            'px-5 py-4',
                            col.width,
                            ALIGN_CLASS[col.align || 'left'],
                            ci === 0 && 'border-l border-border',
                            ci === colCount - 1 && 'border-r border-border',
                            isFirst && 'border-t border-border',
                            isFirst && ci === 0 && r.tl,
                            isFirst && ci === colCount - 1 && r.tr,
                            isLast && 'border-b border-border',
                            isLast && ci === 0 && r.bl,
                            isLast && ci === colCount - 1 && r.br,
                          )}
                        >
                          {col.render(item, idx)}
                        </td>
                      ))}
                    </tr>
                  </React.Fragment>
                );
              })
            )}
            {!loading && data.length > 0 && footer}
          </tbody>
        </table>
      </div>
    </div>
  );
}

/**
 * Grouped variant — single table with per-group visual borders.
 * Uses `border-collapse: separate` with border on first/last td of each group
 * to create the rounded-border-per-group effect while keeping columns aligned.
 */
function GroupedDataTable<T>({
  columns,
  data,
  rowKey,
  loading = false,
  skeletonRows = 5,
  onRowClick,
  activeRowKey,
  emptyContent,
  footer,
  groupConfig,
  className,
}: DataTableProps<T> & { groupConfig: DataTableGroupConfig<T> }) {
  const totalCols = columns.length + 1;
  if (loading) {
    return (
      <div className={cn('flex flex-col gap-4', className)}>
        {Array.from({ length: skeletonRows }).map((_, i) => (
          <Skeleton key={i} className="h-16 w-full rounded" />
        ))}
      </div>
    );
  }
  if (!data.length) {
    return (
      <div className={cn('flex flex-1 items-center justify-center', className)}>
        {emptyContent || <EmptyState />}
      </div>
    );
  }

  const groups = groupData(data, groupConfig.groupBy);

  return (
    <div className={cn('flex flex-col', className)}>
      <div className="overflow-x-auto">
        <table className={cn(TABLE_CLASS, 'border-separate border-spacing-0')}>
          {/* Header */}
          <thead>
            <tr className="text-sm text-text-secondary">
              <th className={cn('px-5 py-3 font-medium', groupConfig.headerWidth)} />
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={cn('px-5 py-3 font-medium', col.width, ALIGN_CLASS[col.align || 'left'])}
                >
                  {col.title}
                </th>
              ))}
            </tr>
          </thead>

          {/* Group bodies */}
          {groups.map(({ key: gKey, items }, gIdx) => {
            const groupLen = items.length;
            return (
              <tbody key={gKey}>
                {/* Spacer row between groups */}
                {gIdx > 0 && (
                  <tr aria-hidden="true">
                    <td colSpan={totalCols} className="h-5 p-0" />
                  </tr>
                )}
                {items.map((item, idx) => {
                  const globalIdx = data.indexOf(item);
                  const isFirst = idx === 0;
                  const isLast = idx === groupLen - 1;
                  const isActive =
                    activeRowKey != null && rowKey(item, globalIdx) === activeRowKey;

                  const borderCls = (colIdx: number) => {
                    const isFirstCol = colIdx === 0;
                    const isLastCol = colIdx === columns.length;
                    const cls: string[] = ['border-border'];

                    if (isFirst) cls.push('border-t');
                    if (isLast) cls.push('border-b');
                    if (isFirstCol) cls.push('border-l');
                    if (isLastCol) cls.push('border-r');
                    if (isFirst && isFirstCol) cls.push('rounded-tl-lg');
                    if (isFirst && isLastCol) cls.push('rounded-tr-lg');
                    if (isLast && isFirstCol) cls.push('rounded-bl-lg');
                    if (isLast && isLastCol) cls.push('rounded-br-lg');

                    return cls.join(' ');
                  };

                  return (
                    <React.Fragment key={rowKey(item, globalIdx)}>
                      {idx > 0 && (
                        <tr aria-hidden="true">
                          <td className={cn('p-0 border-l border-border', groupConfig.headerWidth)} />
                          {columns.map((col, ci) => (
                            <td
                              key={col.key}
                              className={cn(
                                'p-0',
                                ci === columns.length - 1 && 'border-r border-border'
                              )}
                            >
                              <div className="border-t border-border" />
                            </td>
                          ))}
                        </tr>
                      )}
                      <tr
                        className={cn(
                          'text-text-table transition-colors',
                          isActive
                            ? 'bg-surface-secondary'
                            : 'hover:bg-surface-secondary/50',
                          onRowClick && 'cursor-pointer'
                        )}
                        onClick={onRowClick ? () => onRowClick(item, globalIdx) : undefined}
                      >
                        {isFirst ? (
                          <td
                            rowSpan={groupLen * 2 - 1}
                            className={cn(
                              'px-5 py-4 align-top border-l border-t border-b border-border rounded-l-lg',
                              groupConfig.headerWidth
                            )}
                          >
                            {groupConfig.renderGroupHeader(gKey, items)}
                          </td>
                        ) : null}
                        {columns.map((col, ci) => (
                          <td
                            key={col.key}
                            className={cn(
                              'px-5 py-3',
                              col.width,
                              ALIGN_CLASS[col.align || 'left'],
                              borderCls(ci + 1)
                            )}
                          >
                            {col.render(item, globalIdx)}
                          </td>
                        ))}
                      </tr>
                    </React.Fragment>
                  );
                })}
              </tbody>
            );
          })}
          {footer && <tfoot>{footer}</tfoot>}
        </table>
      </div>
    </div>
  );
}
