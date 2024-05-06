import { ref, computed } from "vue";
import { defineStore } from "pinia";

export const useMaterials = defineStore("Materials", () => {
  const visibleDialog = ref(false);
  const headerForm = ref("Tạo mới");
  const model = ref({});
  const submited = ref();

  const files = ref([]);
  const waiting = ref();
  return {
    model,
    headerForm,
    visibleDialog,
    submited,
    files,
    waiting,
  };
});
