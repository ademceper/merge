"use client";

import * as React from "react";
import { useRouter } from "next/navigation";
import { Button } from "@merge/ui/components/button";
import { Input } from "@merge/ui/components/input";
import { Field, FieldContent, FieldError } from "@merge/ui/components/field";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = React.useState("");
  const [password, setPassword] = React.useState("");
  const [isLoading, setIsLoading] = React.useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim() || !password.trim()) {
      return;
    }

    setIsLoading(true);
    // TODO: Implement actual login logic
    setTimeout(() => {
      setIsLoading(false);
      router.push("/");
    }, 1000);
  };

  const handleForgotPassword = () => {
    router.push("/forgot-password");
  };

  const handleSignUp = () => {
    router.push("/register");
  };

  return (
    <main className="min-h-screen bg-white dark:bg-[#101022] flex items-center justify-center px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-sm space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-300">
        {/* Header */}
        <div className="text-center space-y-2">
          <h1 className="text-4xl font-bold uppercase tracking-tight leading-tight text-[#111118] dark:text-white">
            Login
          </h1>
          <p className="text-base font-light leading-relaxed text-[#111118]/70 dark:text-white/70 max-w-[280px] mx-auto">
            Enter your credentials to continue
          </p>
        </div>

        {/* Form */}
        <form onSubmit={handleLogin} className="space-y-5">
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

          {/* Password Input */}
          <Field>
            <FieldContent>
              <Input
                type="password"
                placeholder="Password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete="password"
                className="h-12 rounded-lg px-4 border-[#111118]/10 dark:border-white/10 bg-transparent dark:bg-white/5 text-[#111118] dark:text-white placeholder:text-[#111118]/40 dark:placeholder:text-white/40 focus-visible:border-[#111118] dark:focus-visible:border-white"
              />
            </FieldContent>
          </Field>

          {/* Forgot Password Link */}
          <div className="flex justify-end">
            <Button
              type="button"
              variant="link"
              onClick={handleForgotPassword}
              className="text-sm font-medium text-[#111118]/60 dark:text-white/60 hover:text-[#111118] dark:hover:text-white h-auto p-0">
              Forgot password?
            </Button>
          </div>

          {/* Login Button */}
          <Button
            type="submit"
            disabled={isLoading || !email.trim() || !password.trim()}
            className="w-full h-14 rounded-xl bg-[#111118] dark:bg-white text-white dark:text-[#111118] font-bold uppercase tracking-wider hover:bg-[#111118]/90 dark:hover:bg-white/90 disabled:opacity-50">
            {isLoading ? "Logging in..." : "Login"}
          </Button>
        </form>

        {/* Sign Up Link */}
        <div className="flex items-center justify-center gap-1 text-sm">
          <span className="font-light text-[#111118]/70 dark:text-white/70">
            Don't have an account?
          </span>
          <Button
            type="button"
            variant="link"
            onClick={handleSignUp}
            className="font-medium text-[#111118] dark:text-white h-auto p-0">
            Sign up
          </Button>
        </div>
      </div>
    </main>
  );
}
