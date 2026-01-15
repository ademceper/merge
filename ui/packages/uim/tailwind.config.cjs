/** @type {import('tailwindcss').Config} */
module.exports = {
  theme: {
    extend: {
      colors: {
        background: "hsl(0 0% 100%)",
        foreground: "hsl(0 0% 0%)",
        card: {
          DEFAULT: "hsl(0 0% 100%)",
          foreground: "hsl(0 0% 0%)",
        },
        popover: {
          DEFAULT: "hsl(0 0% 100%)",
          foreground: "hsl(0 0% 0%)",
        },
        primary: {
          DEFAULT: "hsl(0 0% 0%)",
          foreground: "hsl(0 0% 100%)",
        },
        secondary: {
          DEFAULT: "hsl(0 0% 96%)",
          foreground: "hsl(0 0% 9%)",
        },
        muted: {
          DEFAULT: "hsl(0 0% 96%)",
          foreground: "hsl(0 0% 45%)",
        },
        accent: {
          DEFAULT: "hsl(0 0% 96%)",
          foreground: "hsl(0 0% 0%)",
        },
        destructive: {
          DEFAULT: "hsl(0 84.2% 60.2%)",
          foreground: "hsl(0 0% 98%)",
        },
        border: "hsl(0 0% 90%)",
        input: "hsl(0 0% 90%)",
        ring: "hsl(0 0% 0%)",
        chart: {
          1: "hsl(0 0% 20%)",
          2: "hsl(0 0% 40%)",
          3: "hsl(0 0% 60%)",
          4: "hsl(0 0% 80%)",
          5: "hsl(0 0% 90%)",
        },
        sidebar: {
          DEFAULT: "hsl(0 0% 98%)",
          foreground: "hsl(0 0% 0%)",
          primary: "hsl(0 0% 0%)",
          "primary-foreground": "hsl(0 0% 100%)",
          accent: "hsl(0 0% 96%)",
          "accent-foreground": "hsl(0 0% 0%)",
          border: "hsl(0 0% 90%)",
          ring: "hsl(0 0% 0%)",
        },
      },
      borderRadius: {
        lg: "0.5rem",
        md: "0.375rem",
        sm: "0.125rem",
      },
    },
  },
}
