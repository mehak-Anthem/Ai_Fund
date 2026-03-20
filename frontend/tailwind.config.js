/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#06b6d4', // Cyan-500
          light: '#22d3ee', // Cyan-400
          dark: '#0891b2', // Cyan-600
          glow: '#67e8f9', // Cyan-300
        },
        ai: {
          start: '#8b5cf6', // Violet-500
          end: '#06b6d4', // Cyan-500
        },
        surface: {
          DEFAULT: '#0f172a', // Slate-900
          light: '#1e293b', // Slate-800
          lighter: '#334155', // Slate-700
        }
      },
      backgroundImage: {
        'gradient-ai': 'linear-gradient(135deg, var(--tw-gradient-stops))',
        'gradient-radial': 'radial-gradient(var(--tw-gradient-stops))',
      },
      animation: {
        'fadeIn': 'fadeIn 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards',
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'float': 'float 3s ease-in-out infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0', transform: 'translateY(10px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        float: {
          '0%, 100%': { transform: 'translateY(0)' },
          '50%': { transform: 'translateY(-5px)' },
        }
      }
    },
  },
  plugins: [],
}
