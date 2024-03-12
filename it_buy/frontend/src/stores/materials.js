import { ref, computed } from "vue";
import { defineStore } from "pinia";

export const useMaterials = defineStore("Materials", () => {
  const visibleDialog = ref(false);
  const headerForm = ref("Tạo mới");
  const model = ref({});
  return {
    model,
    headerForm,
    visibleDialog
  };
});
