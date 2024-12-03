import { ref, computed } from "vue";
import { defineStore } from "pinia";
import moment from "moment";
import muahangApi from "../api/muahangApi";

export const useMuahang = defineStore("muahang", () => {
  const model = ref({});
  const user_created_by = ref({});
  const tabviewActive = ref(0);
  const datatable = ref([]);
  const nccs = ref([]);
  const nccs_chitiet = ref([]);
  const files = ref([]);
  const QrNhanhang = ref();
  const waiting = ref();
  const list_uynhiemchi = ref([]);
  const chonmua = ref({});
  const list_user_nhanhang = ref([]);
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
    waiting.value = true;
    var res = await muahangApi.get(id);
    var list_ncc = await muahangApi.Getnccs(id);
    var uynhiemchi = res.uynhiemchi;
    var chitiet = res.chitiet;
    var user = res.user_created_by;
    var muahang_chonmua = res.muahang_chonmua;
    res.date = res.date ? moment(res.date).format("YYYY-MM-DD") : null;
    res.nhacungcap_id = muahang_chonmua ? muahang_chonmua.ncc_id : null;
    delete res.chitiet;
    delete res.uynhiemchi;
    delete res.user_created_by;
    delete res.muahang_chonmua;
    user_created_by.value = user;
    model.value = res;
    datatable.value = chitiet;
    nccs.value = list_ncc;
    chonmua.value = muahang_chonmua;
    list_uynhiemchi.value = uynhiemchi;

    waiting.value = false;
  };
  const getQrNhanhang = async (id) => {
    var res = await muahangApi.QrNhanhang(id);
    if (res.success) {
      QrNhanhang.value = res.link;
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
    user_created_by,
    chonmua,
    load_data,
    getQrNhanhang,
    reset,
  };
});
