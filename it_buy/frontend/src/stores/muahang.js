import { ref, computed } from "vue";
import { defineStore } from "pinia";

export const useMuahang = defineStore("muahang", () => {
  const model = ref({});
  const datatable = ref([]);
  const nccs = ref([]);
  const nccs_chitiet = ref([]);
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
    nccs,
    nccs_chitiet,
    datatable,
    list_add,
    list_update,
    list_delete,
    reset,
  };
});
