'use client';

function Bone({ className = '' }: { className?: string }) {
  return <div className={`animate-pulse rounded bg-surface-secondary ${className}`} />;
}

export function EventShopSkeleton() {
  return (
    <div className="flex flex-col gap-3 md:gap-5 w-full">
      {/* NotificationBar 骨架 */}
      {/* <Bone className="h-9 w-full rounded" /> */}

      <div className="flex flex-col md:flex-row gap-3 md:gap-5 w-full flex-1 min-h-0">
        {/* 左侧：UserInfoCard + Sidebar */}
        <div className="flex flex-col gap-3 md:gap-5 w-full md:w-[320px] shrink-0">
          {/* UserInfoCard 骨架 */}
          <div className="bg-surface border border-border rounded flex flex-col items-center justify-center py-5 w-full">
            <div className="flex flex-col gap-5 items-center">
              <div className="flex flex-col gap-2 items-center">
                <Bone className="size-20 rounded-full" />
                <Bone className="h-9 w-16" />
              </div>
              <div className="flex flex-col gap-1 items-center px-2 w-[200px]">
                <div className="flex items-center justify-between w-full leading-relaxed">
                  <Bone className="h-5 w-12" />
                  <Bone className="h-7 w-24" />
                </div>
                <div className="flex items-center justify-between w-full leading-relaxed">
                  <Bone className="h-5 w-16" />
                  <Bone className="h-6 w-16" />
                </div>
              </div>
            </div>
          </div>

          {/* Sidebar 骨架 */}
          <div className="bg-surface border border-border rounded p-5 flex flex-col gap-5 w-full">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="flex flex-col gap-5">
                <Bone className={`h-12 w-full rounded ${i === 0 ? 'opacity-80' : 'opacity-40'}`} />
                {(i === 2 || i === 4) && <div className="h-px bg-border w-full" />}
              </div>
            ))}
          </div>
        </div>

        {/* 右侧：Banner + ProductGrid */}
        <div className="flex flex-1 flex-col gap-5 min-w-0">
          {/* Banner 骨架 */}
          <Bone className="h-[228px] w-full brightness-95 dark:brightness-100" />

          {/* ProductGrid 骨架 */}
          <div className="bg-surface rounded p-5 flex flex-col gap-5 flex-1">
            {/* 分类 tabs 骨架 */}
            <div className="flex flex-col items-start overflow-hidden w-full">
              <div className="flex items-start gap-5 md:gap-10 pb-2 w-full">
                <div className="flex flex-col gap-[18px] items-center shrink-0">
                  <Bone className="h-7 w-10" />
                  <Bone className="h-0.5 w-10" />
                </div>
                {Array.from({ length: 4 }).map((_, i) => (
                  <Bone key={i} className="h-7 w-24 shrink-0" />
                ))}
              </div>
              <div className="h-px bg-border w-full" />
            </div>

            {/* 商品卡片骨架 */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
              {Array.from({ length: 4 }).map((_, i) => (
                <div key={i} className="border border-border rounded-xl overflow-hidden p-5 md:p-10">
                  <div className="flex gap-3 items-center w-full">
                    <Bone className="size-[100px] md:size-[150px] rounded-2xl shrink-0" />
                    <div className="flex flex-col justify-between flex-1 py-2 md:py-5 min-h-[100px] md:min-h-[150px]">
                      <Bone className="h-4 w-3/4" />
                      <div className="flex items-center justify-between">
                        <Bone className="h-6 w-20" />
                        <Bone className="h-8 w-20 rounded" />
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
