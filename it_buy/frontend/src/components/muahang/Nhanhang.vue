﻿<template>
  <div class="row my-5 align-items-end" v-if="model.is_dathang == true">
    <div class="col-md-4 form-group">
      <b class="">Ngày giao hàng dự kiến:</b>
      <div class="mt-2">
        <Calendar
          v-model="model.date"
          dateFormat="yy-mm-dd"
          class="date-custom"
          :manualInput="false"
          showIcon
          :disabled="readonly"
          @update:modelValue="changeNgaygiaohang"
        />
      </div>
    </div>
    <div class="col-md-6 form-group row align-items-center">
      <b class="col-md-3 text-md-right">Mã nhận hàng:</b>
      <div class="col-md-8">
        <a :href="QrNhanhang" :download="model.code" class="text-blue">
          <img :src="QrNhanhang" style="width: 100px" />
        </a>
        hoặc
        <a
          :href="'/muahang/nhanhang/' + model.id"
          target="_blank"
          class="text-blue"
          >Link</a
        >
        <!-- <div class="col-12 pt-3 text-center">
        <Button label="Tải xuống" icon="fas fa-download" @click="download" />
      </div> -->
      </div>
    </div>

    <div class="col-md-2 form-group row align-items-center">
      <Button
        label="Thông báo nhận hàng"
        size="small"
        @click="openThongbao"
      ></Button>
      <Dialog
        v-model:visible="visibleDialog"
        header="Thông báo nhận hàng"
        :modal="true"
        class="p-fluid"
        style="width: 75vw"
        :breakpoints="{ '1199px': '75vw', '575px': '95vw' }"
      >
        <div class="row">
          <b class="col-12">Người nhận:</b>
          <div class="col-12 mt-2">
            <UserDepartmentTreeSelect
              multiple
              required
              v-model="listuser"
              :name="'user_nhanhang'"
            >
            </UserDepartmentTreeSelect>
          </div>
        </div>
        <div class="row">
          <b class="col-12">Ghi chú:</b>
          <div class="col-12 mt-2">
            <textarea
              v-model="note"
              class="form-control form-control-sm"
              rows="10"
            ></textarea>
          </div>
        </div>
        <template #footer>
          <Button
            label="Thông báo"
            icon="pi pi-check"
            class="p-button-text"
            @click="thongbao"
            size="small"
          ></Button>
        </template>
      </Dialog>
    </div>
    <div class="col-12">
      <FormMuahangNhanhangVue></FormMuahangNhanhangVue>
    </div>
    <div class="col-md-12 mt-3">
      <Button
        label="Lưu lại"
        icon="pi pi-save"
        class="p-button-success p-button-sm mr-2"
        @click.prevent="save()"
      ></Button>
    </div>
  </div>
</template>
<script setup>
import { computed, onMounted, ref } from "vue";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import muahangApi from "../../api/muahangApi";
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import UserDepartmentTreeSelect from "../TreeSelect/UserDepartmentTreeSelect.vue";
import FormMuahangNhanhangVue from "../Datatable/FormMuahangNhanhang.vue";
import { useToast } from "primevue/usetoast";
import { useRoute } from "vue-router";
const store_muahang = useMuahang();
const { model, QrNhanhang, datatable, waiting } = storeToRefs(store_muahang);
const visibleDialog = ref();
const minDate = ref(new Date());
const listuser = ref([]);
const note = ref();
const toast = useToast();
const route = useRoute();

const changeNgaygiaohang = async () => {
  await muahangApi.save(model.value);
};
const readonly = computed(() => {
  if (model.value.date_finish) return true;
  return false;
});
const openThongbao = async () => {
  note.value = "";
  listuser.value = await muahangApi.getUserNhanhang(model.value.id);
  visibleDialog.value = true;
};
const thongbao = async () => {
  visibleDialog.value = false;
  var res = await muahangApi.thongbao({
    muahang_id: model.value.id,
    list_user: listuser.value,
    note: note.value,
  });
  if (res.success) {
    toast.add({
      severity: "success",
      summary: "Thành công",
      detail: "Thành công",
      life: 3000,
    });
  }
};
const save = async () => {
  var items = [];

  for (var item of datatable.value) {
    delete item.muahang;
    delete item.muahang_ncc_chitiet;
    items.push(item);
  }

  // console.log(items);
  // return false;
  muahangApi
    .saveChitiet({ muahang_id: model.value.id, list: items })
    .then((response) => {
      waiting.value = false;
      if (response.success) {
        toast.add({
          severity: "success",
          summary: "Thành công!",
          detail: "Thay đổi thành công",
          life: 3000,
        });
        location.reload();
      }
    });
};
onMounted(() => {
  store_muahang.getQrNhanhang(route.params.id);
});
</script>

<style lang="scss"></style>
