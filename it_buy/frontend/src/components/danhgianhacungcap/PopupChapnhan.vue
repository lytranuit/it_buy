<template>
  <Dialog
    v-model:visible="visibleDanhgia"
    modal
    header="Ghi chú"
    style="width: 50vw"
    :breakpoints="{ '1199px': '75vw', '575px': '95vw' }"
  >
    <textarea
      class="form-control form-control-sm"
      v-model="editRow.note"
      rows="5"
    ></textarea>

    <div class="d-flex justify-content-center mt-2">
      <Button
        type="button"
        label="Hủy bỏ"
        severity="secondary"
        @click="visibleDanhgia = false"
        size="small"
        class="mr-2"
      ></Button>
      <Button
        type="button"
        label="Chấp nhận"
        severity="success"
        @click="Chapnhan"
        size="small"
      ></Button>
    </div>
  </Dialog>
</template>
<script setup>
import { storeToRefs } from "pinia";
import { useDanhgianhacungcap } from "../../stores/danhgianhacungcap";
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import danhgianhacungcapApi from "../../api/danhgianhacungcapApi";
import { useRoute } from "vue-router";
const route = useRoute();
const store_danhgianhacungcap = useDanhgianhacungcap();
const { visibleDanhgia, editRow } = storeToRefs(store_danhgianhacungcap);
const Chapnhan = () => {
  danhgianhacungcapApi
    .chapnhanDanhgia(editRow.value.id, editRow.value.note)
    .then((res) => {
      visibleDanhgia.value = false;
      store_danhgianhacungcap.getDanhgia(route.params.id);
    });
};
</script>