import { fileURLToPath, URL } from "node:url";

import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import Pages from "vite-plugin-pages";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    Pages({
      dirs: "src/views",
    }),
  ],
  resolve: {
    alias: [
      { find: '~@syncfusion', replacement: './node_modules/@syncfusion' },
      { find: '@', replacement: fileURLToPath(new URL("./src", import.meta.url)) }
    ]
  },
});
