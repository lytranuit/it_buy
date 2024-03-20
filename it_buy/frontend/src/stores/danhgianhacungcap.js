import { ref, computed } from "vue";
import { defineStore } from "pinia";

export const useDanhgianhacungcap = defineStore("Danhgianhacungcap", () => {
  const visibleDialog = ref(false);
  const headerForm = ref("Tạo mới");
  const model = ref({});
  const submited = ref();
  const user_created_by = ref({});
  const files = ref([]);
  const waiting = ref();
  return {
    model,
    user_created_by,
    files,
    waiting,
    headerForm,
    visibleDialog,
    submited,
  };
});
