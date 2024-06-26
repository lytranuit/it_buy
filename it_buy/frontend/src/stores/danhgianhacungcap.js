import { ref, computed } from "vue";
import { defineStore } from "pinia";
import danhgianhacungcapApi from "../api/danhgianhacungcapApi";

export const useDanhgianhacungcap = defineStore("Danhgianhacungcap", () => {
  const visibleDialog = ref(false);
  const visibleDanhgia = ref(false);
  const headerForm = ref("Tạo mới");
  const model = ref({});
  const editRow = ref({});
  const submited = ref();
  const user_created_by = ref({});
  const files = ref([]);
  const waiting = ref();
  const danhgia = ref([]);
  const list_delete = ref();
  const list_add = computed(() => {
    return danhgia.value.filter((item) => {
      return item.ids;
    });
  });
  const list_update = computed(() => {
    return danhgia.value.filter((item) => {
      return !item.ids;
    });
  });
  const openNew = () => {
    model.value = {};
    headerForm.value = "Tạo mới";
    visibleDialog.value = true;
  };
  const openChapnhan = (selected) => {
    visibleDanhgia.value = true;
    editRow.value = selected
  };
  const getDanhgia = (id) => {
    danhgianhacungcapApi.getDanhgia(id).then((res) => {
      danhgia.value = res;
    });
  };
  return {
    model,
    user_created_by,
    files,
    waiting,
    headerForm,
    visibleDialog,
    visibleDanhgia,
    submited,
    danhgia,
    list_add,
    list_update,
    list_delete,
    editRow,
    openNew,
    openChapnhan,
    getDanhgia
  };
});
