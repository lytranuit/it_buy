import { ref, computed } from "vue";
import { defineStore } from "pinia";
import moment from "moment";
import muahangApi from "../api/muahangApi";

export const useMuahang = defineStore("muahang", () => {
  const model = ref({});
  const tabviewActive = ref(0);
  const datatable = ref([]);
  const nccs = ref([]);
  const nccs_chitiet = ref([]);
  const files = ref([]);
  const QrNhanhang = ref([]);
  const waiting = ref();
  const list_uynhiemchi = ref([]);
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
  const load_data = async (id) => {
    var res = await muahangApi.get(id);
    var uynhiemchi = res.uynhiemchi;
    var chitiet = res.chitiet;
    var list_ncc = res.nccs;
    res.date = res.date ? moment(res.date).format("YYYY-MM-DD") : null;
    delete res.chitiet;
    delete res.nccs;
    delete res.uynhiemchi;
    delete res.user_created_by;
    model.value = res;
    datatable.value = chitiet;
    nccs.value = list_ncc;
    list_uynhiemchi.value = uynhiemchi;
  };
  const getQrNhanhang = async (id) => {
    var res = await muahangApi.QrNhanhang(id);
    if (res.success) {
      QrNhanhang.value = res.list;
    }
  };
  return {
    model,
    nccs,
    nccs_chitiet,
    datatable,
    list_add,
    list_update,
    list_delete,
    files,
    list_uynhiemchi,
    waiting,
    tabviewActive,
    QrNhanhang,
    load_data,
    getQrNhanhang,
    reset,
  };
});
