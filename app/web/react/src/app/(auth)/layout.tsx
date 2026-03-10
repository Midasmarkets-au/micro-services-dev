import { AuthHeader } from '@/components/layout/AuthHeader';
import { AuthFooter } from '@/components/layout/AuthFooter';

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen bg-background">
      <AuthHeader />
      <main className="container-responsive flex min-h-screen items-center justify-center pb-16 pt-20">
        {children}
      </main>
      <AuthFooter />
    </div>
  );
}
