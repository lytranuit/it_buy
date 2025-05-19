import { ref, computed } from "vue";
import { defineStore } from "pinia";

export const useVattu = defineStore("vattu", () => {
  const model = ref({});
  const datatable = ref([]);
  const waiting = ref();
  const start_event = ref(false);
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
    start_event.value = false;
    return true;
  };
  return {
    model,
    datatable,
    list_add,
    list_update,
    list_delete,
    waiting,
    start_event,
    reset,
  };
});
