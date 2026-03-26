import React from 'react';
import { motion } from 'framer-motion';

const TypingIndicator: React.FC = () => {
  return (
    <div className="flex mb-8 animate-fadeIn">
      <div className="flex gap-1.5 px-5 py-4 bg-bg-secondary border border-border-primary rounded-[24px] rounded-tl-[4px]">
        {[0, 1, 2].map((i) => (
          <motion.div
            key={i}
            animate={{
              y: [0, -4, 0],
              opacity: [0.4, 1, 0.4]
            }}
            transition={{
              duration: 0.8,
              repeat: Infinity,
              delay: i * 0.15,
              ease: "easeInOut"
            }}
            className="w-1.5 h-1.5 bg-indigo-500 rounded-full"
          />
        ))}
      </div>
    </div>
  );
};

export default TypingIndicator;
