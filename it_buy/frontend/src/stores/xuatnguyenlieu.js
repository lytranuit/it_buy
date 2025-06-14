import { ref, computed } from "vue";
import { defineStore } from "pinia";

export const usexuatnguyenlieu = defineStore("xuatnguyenlieu", () => {
  const model = ref({});
  const user_created_by = ref({});
  const datatable = ref([]);
  const list_nhanhang = ref([]);
  const list_muahang = ref([]);
  const files = ref([]);
  const tabviewActive = ref();
  const waiting = ref();
  const list_add = computed(() => {
    return datatable.value.filter((item) => {
      return item.ids;
    });
  });
  const list_update = computed(() => {
    return datatable.value.filter((item) => {
      return !item.ids;
    });
  });
  const list_delete = ref();
  const reset = () => {
    model.value = {};
    datatable.value = [];
    return true;
  };
  return {
    model,
    user_created_by,
    datatable,
    list_add,
    list_update,
    list_delete,
    list_nhanhang,
    list_muahang,
    files,
    tabviewActive,
    waiting,
    reset,
  };
});
