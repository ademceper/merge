"use client";

import * as React from "react";
import { useRouter } from "next/navigation";
import { Button } from "@merge/ui/components/button";
import { Input } from "@merge/ui/components/input";
import { Field, FieldContent } from "@merge/ui/components/field";

export default function ForgotPasswordPage() {
  const router = useRouter();
  const [email, setEmail] = React.useState("");
  const [isLoading, setIsLoading] = React.useState(false);
  const [isEmailSent, setIsEmailSent] = React.useState(false);

  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim()) {
      return;
    }

    setIsLoading(true);
    // TODO: Implement actual password reset logic
    setTimeout(() => {
      setIsLoading(false);
      setIsEmailSent(true);
    }, 1000);
  };

  const handleBackToLogin = () => {
    router.push("/login");
  };

  const handleResendEmail = async () => {
    if (!email.trim()) {
      return;
    }

    setIsLoading(true);
    // TODO: Implement actual password reset logic
    setTimeout(() => {
      setIsLoading(false);
      setIsEmailSent(true);
    }, 1000);
  };

  return (
    <main className="min-h-screen bg-white dark:bg-[#101022] flex items-center justify-center px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-sm space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-300">
        {/* Header */}
        <div className="text-center space-y-2">
          <h1 className="text-4xl font-bold uppercase tracking-tight leading-tight text-[#111118] dark:text-white">
            {isEmailSent ? "Check Email" : "Forgot Password"}
          </h1>
          <p className="text-base font-light leading-relaxed text-[#111118]/70 dark:text-white/70 max-w-[280px] mx-auto">
            {isEmailSent
              ? "We've sent a password reset link to your email address"
              : "Enter your email address and we'll send you a link to reset your password"}
          </p>
        </div>

        {!isEmailSent ? (
          <>
            {/* Form */}
            <form onSubmit={handleResetPassword} className="space-y-5">
              {/* Email Input */}
              <Field>
                <FieldContent>
                  <Input
                    type="email"
                    placeholder="Email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    autoComplete="email"
                    className="h-12 rounded-lg px-4 border-[#111118]/10 dark:border-white/10 bg-transparent dark:bg-white/5 text-[#111118] dark:text-white placeholder:text-[#111118]/40 dark:placeholder:text-white/40 focus-visible:border-[#111118] dark:focus-visible:border-white"
                  />
                </FieldContent>
              </Field>

              {/* Reset Password Button */}
              <Button
                type="submit"
                disabled={isLoading || !email.trim()}
                className="w-full h-14 rounded-xl bg-[#111118] dark:bg-white text-white dark:text-[#111118] font-bold uppercase tracking-wider hover:bg-[#111118]/90 dark:hover:bg-white/90 disabled:opacity-50">
                {isLoading ? "Sending..." : "Send Reset Link"}
              </Button>
            </form>
          </>
        ) : (
          <>
            {/* Success State */}
            <div className="space-y-6">
              <div className="flex items-center justify-center">
                <div className="w-20 h-20 rounded-full flex items-center justify-center bg-[#111118]/10 dark:bg-white/10">
                  <span className="text-4xl text-[#111118] dark:text-white">✓</span>
                </div>
              </div>

              {/* Resend Email Button */}
              <Button
                type="button"
                onClick={handleResendEmail}
                disabled={isLoading}
                variant="outline"
                className="w-full h-14 rounded-xl border-[#111118]/20 dark:border-white/20 bg-transparent text-[#111118] dark:text-white font-bold uppercase tracking-wider hover:bg-[#111118]/5 dark:hover:bg-white/5 disabled:opacity-50">
                {isLoading ? "Sending..." : "Resend Email"}
              </Button>
            </div>
          </>
        )}

        {/* Back to Login Link */}
        <div className="flex items-center justify-center gap-1 text-sm">
          <span className="font-light text-[#111118]/70 dark:text-white/70">
            Remember your password?
          </span>
          <Button
            type="button"
            variant="link"
            onClick={handleBackToLogin}
            className="font-medium text-[#111118] dark:text-white h-auto p-0">
            Login
          </Button>
        </div>
      </div>
    </main>
  );
}
