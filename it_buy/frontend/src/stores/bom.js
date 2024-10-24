import { ref, computed } from "vue";
import { defineStore } from "pinia";

export const useBom = defineStore("Bom", () => {
  const visibleDialog = ref(false);
  const headerForm = ref("Tạo mới");
  const model = ref({});
  const datatable = ref([]);
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
  const list_delete = ref([]);
  const reset = () => {
    model.value = {};
    datatable.value = [];
    list_delete.value = [];
  }
  return {
    model,
    headerForm,
    visibleDialog,
    datatable,
    list_delete,
    list_add,
    list_update,
    reset
  };
});
