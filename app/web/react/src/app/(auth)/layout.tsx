import { AuthHeader } from '@/components/layout/AuthHeader';
import { AuthFooter } from '@/components/layout/AuthFooter';

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen flex-col bg-background">
      <AuthHeader />
      <main className="container-responsive pointer-events-none relative z-10 flex flex-1 items-center justify-center pt-20">
        <div className="pointer-events-auto">
          {children}
        </div>
      </main>
      <AuthFooter />
    </div>
  );
}
