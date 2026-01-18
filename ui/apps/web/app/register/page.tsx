"use client";

import * as React from "react";
import { useRouter } from "next/navigation";
import { Button } from "@merge/ui/components/button";
import { Input } from "@merge/ui/components/input";
import { Field, FieldContent, FieldError } from "@merge/ui/components/field";

export default function RegisterPage() {
  const router = useRouter();
  const [name, setName] = React.useState("");
  const [email, setEmail] = React.useState("");
  const [password, setPassword] = React.useState("");
  const [confirmPassword, setConfirmPassword] = React.useState("");
  const [isLoading, setIsLoading] = React.useState(false);

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !email.trim() || !password.trim() || !confirmPassword.trim()) {
      return;
    }

    if (password !== confirmPassword) {
      // TODO: Show error message
      return;
    }

    setIsLoading(true);
    // TODO: Implement actual register logic
    setTimeout(() => {
      setIsLoading(false);
      router.push("/");
    }, 1000);
  };

  const handleLogin = () => {
    router.push("/login");
  };

  const isFormValid = name.trim() && email.trim() && password.trim() && confirmPassword.trim() && password === confirmPassword;

  return (
    <main className="min-h-screen bg-white dark:bg-[#101022] flex items-center justify-center px-4 sm:px-6 lg:px-8 py-8">
      <div className="w-full max-w-sm space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-300">
        {/* Header */}
        <div className="text-center space-y-2">
          <h1 className="text-4xl font-bold uppercase tracking-tight leading-tight text-[#111118] dark:text-white">
            Register
          </h1>
          <p className="text-base font-light leading-relaxed text-[#111118]/70 dark:text-white/70 max-w-[280px] mx-auto">
            Create your account to get started
          </p>
        </div>

        {/* Form */}
        <form onSubmit={handleRegister} className="space-y-5">
          {/* Name Input */}
          <Field>
            <FieldContent>
              <Input
                type="text"
                placeholder="Full Name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                autoComplete="name"
                className="h-12 rounded-lg px-4 border-[#111118]/10 dark:border-white/10 bg-transparent dark:bg-white/5 text-[#111118] dark:text-white placeholder:text-[#111118]/40 dark:placeholder:text-white/40 focus-visible:border-[#111118] dark:focus-visible:border-white"
              />
            </FieldContent>
          </Field>

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
                autoComplete="new-password"
                className="h-12 rounded-lg px-4 border-[#111118]/10 dark:border-white/10 bg-transparent dark:bg-white/5 text-[#111118] dark:text-white placeholder:text-[#111118]/40 dark:placeholder:text-white/40 focus-visible:border-[#111118] dark:focus-visible:border-white"
              />
            </FieldContent>
          </Field>

          {/* Confirm Password Input */}
          <Field data-invalid={confirmPassword.length > 0 && password !== confirmPassword}>
            <FieldContent>
              <Input
                type="password"
                placeholder="Confirm Password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                autoComplete="new-password"
                className="h-12 rounded-lg px-4 border-[#111118]/10 dark:border-white/10 bg-transparent dark:bg-white/5 text-[#111118] dark:text-white placeholder:text-[#111118]/40 dark:placeholder:text-white/40 focus-visible:border-[#111118] dark:focus-visible:border-white"
              />
              {confirmPassword.length > 0 && password !== confirmPassword && (
                <FieldError className="text-red-600 dark:text-red-500">
                  Passwords do not match
                </FieldError>
              )}
            </FieldContent>
          </Field>

          {/* Register Button */}
          <Button
            type="submit"
            disabled={isLoading || !isFormValid}
            className="w-full h-14 rounded-xl bg-[#111118] dark:bg-white text-white dark:text-[#111118] font-bold uppercase tracking-wider hover:bg-[#111118]/90 dark:hover:bg-white/90 disabled:opacity-50">
            {isLoading ? "Creating account..." : "Register"}
          </Button>
        </form>

        {/* Login Link */}
        <div className="flex items-center justify-center gap-1 text-sm">
          <span className="font-light text-[#111118]/70 dark:text-white/70">
            Already have an account?
          </span>
          <Button
            type="button"
            variant="link"
            onClick={handleLogin}
            className="font-medium text-[#111118] dark:text-white h-auto p-0">
            Login
          </Button>
        </div>
      </div>
    </main>
  );
}
