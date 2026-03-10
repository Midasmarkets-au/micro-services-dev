'use client';

import { useEffect, useRef, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { PageLoading } from '@/components/ui';
import { useUserStore } from '@/stores/userStore';
import { setToken as setTokenAction } from '@/actions';

type Status = 'idle' | 'loading' | 'success' | 'error';

export default function SetTokenPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState<Status>('idle');
  const [errorMessage, setErrorMessage] = useState<string>('');
  const clearStore = useUserStore((state) => state.clearStore);
  const isProcessing = useRef(false);
  
  // 获取 token 参数
  const token = searchParams.get('token');

  useEffect(() => {
    // 防止重复执行
    if (isProcessing.current) return;
    
    if (!token) {
      return;
    }

    isProcessing.current = true;
    setStatus('loading');

    const handleSetToken = async () => {
      try {
        // 清除之前的用户数据
        clearStore();
        
        // 清除 localStorage 中的用户缓存（但保留主题设置）
        if (typeof window !== 'undefined') {
          localStorage.removeItem('user-storage');
          sessionStorage.clear();
        }

        // 调用 Server Action 设置 token
        const result = await setTokenAction({ token });

        if (result.success) {
          setStatus('success');
          // 重定向到 dashboard
          router.replace('/dashboard');
        } else {
          setStatus('error');
          setErrorMessage(result.message || result.error || 'Token 设置失败');
        }
      } catch (err) {
        console.error('[Set Token] Error:', err);
        setStatus('error');
        setErrorMessage('设置 Token 时发生错误');
      }
    };

    handleSetToken();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  // 缺少 token 参数
  if (!token) {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-background">
        <div className="flex items-center gap-1">
          <span className="font-brand text-5xl font-bold text-primary">M</span>
          <span className="font-brand text-5xl font-bold text-primary">D</span>
          <span className="font-brand text-5xl font-bold text-primary">M</span>
        </div>
        <p className="text-lg text-red-500">缺少 token 参数</p>
        <button
          onClick={() => router.push('/sign-in')}
          className="rounded bg-primary px-6 py-2 text-white hover:bg-primary-hover"
        >
          返回登录
        </button>
      </div>
    );
  }

  // 显示错误
  if (status === 'error') {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-background">
        <div className="flex items-center gap-1">
          <span className="font-brand text-5xl font-bold text-primary">M</span>
          <span className="font-brand text-5xl font-bold text-primary">D</span>
          <span className="font-brand text-5xl font-bold text-primary">M</span>
        </div>
        <p className="text-lg text-red-500">{errorMessage}</p>
        <button
          onClick={() => router.push('/sign-in')}
          className="rounded bg-primary px-6 py-2 text-white hover:bg-primary-hover"
        >
          返回登录
        </button>
      </div>
    );
  }

  // 显示加载状态
  return <PageLoading />;
}
