<template>
  <div id="TablemuahangNhanhang">
    <DataTable showGridlines :value="modelncc" ref="dt" class="p-datatable-ct" :rowHover="true" :loading="loading"
      responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:selection="selected">
      <!-- <template #header>
                <div class="d-inline-flex" style="width:200px" v-if="model.status_id == 1">
                    <Button label="Thêm" icon="pi pi-plus" class="p-button-success p-button-sm mr-2"
                        @click="addRow"></Button>
                    <Button label="Xóa" icon="pi pi-trash" class="p-button-danger p-button-sm"
                        :disabled="!selected || !selected.length" @click="confirmDeleteSelected"></Button>

                </div>
                <div class="d-inline-flex float-right">
                </div>
            </template> -->

      <template #empty>
        <div class="text-center">Không có dữ liệu.</div>
      </template>
      <Column v-for="col in columns" :field="col.data" :header="col.label" :key="col.data" :showFilterMatchModes="false"
        :class="col.data">
        <template #body="slotProps">
          <template v-if="col.data == 'mahh'">
            {{ slotProps.data["mahh"] }}
          </template>

          <template v-else-if="
            col.data == 'date_nhanhang' &&
            slotProps.data.user_nhanhang_id == user.id
          ">
            <Calendar v-model="slotProps.data[col.data]" dateFormat="yy-mm-dd" class="date-custom" :manualInput="false"
              showIcon :disabled="is_readonly" />
          </template>

          <template v-else-if="
            col.data == 'soluong_nhanhang' &&
            slotProps.data.user_nhanhang_id == user.id
          ">
            <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm" :disabled="is_readonly"
              :maxFractionDigits="2" />
          </template>

          <template v-else-if="
            col.data == 'note_nhanhang' &&
            slotProps.data.user_nhanhang_id == user.id
          ">
            <textarea v-model="slotProps.data[col.data]" class="form-control form-control-sm"
              :disabled="is_readonly"></textarea>
          </template>

          <template v-else-if="
            col.data == 'status_nhanhang' &&
            slotProps.data.user_nhanhang_id == user.id
          ">
            <select class="form-control form-control-sm" v-model="slotProps.data[col.data]" :disabled="is_readonly"
              @change="changeNhanhang(slotProps.data)">
              <option value="0">Chưa nhận hàng</option>
              <option value="1">Đã nhận hàng</option>
              <option value="2">Khiếu nại</option>
            </select>
          </template>

          <template v-else-if="col.data == 'user_nhanhang'">
            <div v-if="slotProps.data['user_nhanhang']" class="d-flex">
              <Avatar :image="slotProps.data.user_nhanhang.image_url" :title="slotProps.data.user_nhanhang.FullName"
                size="small" shape="circle" />
              <span class="align-self-center ml-2">{{
                slotProps.data.user_nhanhang.FullName
              }}</span>
            </div>
          </template>
          <template v-else>
            {{ slotProps.data[col.data] }}
          </template>
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup>
import { onMounted, ref, watch, computed } from "vue";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import InputNumber from "primevue/inputnumber";
import Avatar from "primevue/avatar";
import { storeToRefs } from "pinia";

import { useDutru } from "../../stores/dutru";

import Calendar from "primevue/calendar";
import { useAuth } from "../../stores/auth";
const store_dutru = useDutru();
const { list_nhanhang, model } = storeToRefs(store_dutru);

const store_auth = useAuth();
const { user } = storeToRefs(store_auth);

const minDate = ref(new Date());
const loading = ref(false);
const selected = ref();
const columns = ref([
  {
    label: "Mã",
    data: "mahh",
    className: "text-center",
  },
  {
    label: "Tên",
    data: "tenhh",
    className: "text-center",
  }, {
    label: "Grade",
    data: "grade",
    className: "text-center",
  },
  {
    label: "Nhà sản xuất",
    data: "nhasx",
    className: "text-center",
  },
  {
    label: "Leadtime",
    data: "leadtime",
    className: "text-center",
  },
  {
    label: "MOQ",
    data: "moq",
    className: "text-center",
  },
  {
    label: "ĐVT",
    data: "dvt",
    className: "text-center",
  },
  {
    label: "Số lượng",
    data: "soluong",
    className: "text-center",
  },
  {
    label: "Số lượng nhận hàng (*)",
    data: "soluong_nhanhang",
    className: "text-center",
  },
  {
    label: "Ngày nhận hàng (*)",
    data: "date_nhanhang",
    className: "text-center",
  },
  {
    label: "Ghi chú (*)",
    data: "note_nhanhang",
    className: "text-center",
  },
  {
    label: "Trạng thái (*)",
    data: "status_nhanhang",
    className: "text-center",
  },
  {
    label: "Người nhận hàng",
    data: "user_nhanhang",
    className: "text-center",
  },
]);

const is_readonly = computed(() => {
  if (model.value.date_finish) {
    return true;
  }
  return false;
});
const changeNhanhang = (row) => {
  if (row.status_nhanhang == 1) {
    row.user_nhanhang_id = user.value.id;
  }
};
const props = defineProps({
  index: Number,
});
const modelncc = computed(() => {
  return list_nhanhang.value[props.index].items;
});
onMounted(() => { });
</script>

<style>
.mahh {
  max-width: 300px;
}
</style>