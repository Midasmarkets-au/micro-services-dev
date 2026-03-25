'use client';

import { Logo, ThemeToggle, LanguageToggle } from '@/components/ui';

export function AuthHeader() {
  return (
    <header className="fixed left-0 right-0 top-0">
      <div className="container-auth-responsive flex h-20 items-center justify-between">
        <Logo />
        <div className="flex items-center gap-10 md:gap-10">
          <ThemeToggle />
          <LanguageToggle />
        </div>
      </div>
    </header>
  );
}
