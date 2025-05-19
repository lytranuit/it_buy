<template>
  <div id="TablemuahangNhanhang">
    <DataTable showGridlines :value="datatable" ref="dt" id="table-muahang-nhanhang" class="p-datatable-ct"
      :rowHover="true" :loading="loading" responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand"
      v-model:selection="selected">
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
          <template v-if="
            editable == true &&
            col.data == 'date_nhanhang' &&
            slotProps.data.user_nhanhang_id == user.id
          ">
            <Calendar v-model="slotProps.data[col.data]" dateFormat="yy-mm-dd" class="date-custom" :manualInput="false"
              showIcon :disabled="model.date_finish" />
          </template>
          <template v-else-if="editable == false && col.data == 'grade'">
            <!-- Cung ứng điền -->
            <input v-model="slotProps.data[col.data]" class="form-control" :disabled="model.date_finish" />
          </template>
          <template v-else-if="editable == false && col.data == 'nhasx'">
            <input v-model="slotProps.data[col.data]" class="form-control" :disabled="model.date_finish" />
          </template>
          <template v-else-if="editable == false && col.data == 'leadtime'">
            <input v-model="slotProps.data[col.data]" class="form-control" :disabled="model.date_finish" />
          </template>
          <template v-else-if="editable == false && col.data == 'mahh'">
            <input v-model="slotProps.data[col.data]" class="form-control form-control-sm"
              :disabled="model.date_finish" />
          </template>
          <template v-else-if="
            editable == true &&
            col.data == 'soluong_nhanhang' &&
            slotProps.data.user_nhanhang_id == user.id
          ">
            <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm" :disabled="model.date_finish"
              :maxFractionDigits="2" />
          </template>

          <template v-else-if="
            editable == true &&
            col.data == 'note_nhanhang' &&
            slotProps.data.user_nhanhang_id == user.id
          ">
            <textarea v-model="slotProps.data[col.data]" class="form-control form-control-sm"
              :disabled="model.date_finish"></textarea>
          </template>

          <template v-else-if="
            editable == true &&
            col.data == 'status_nhanhang' &&
            slotProps.data.user_nhanhang_id == user.id
          ">
            <select class="form-control form-control-sm" v-model="slotProps.data[col.data]"
              :disabled="model.date_finish" @change="changeNhanhang(slotProps.data)">
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
          <template v-else-if="
            col.data == 'status_nhanhang' && slotProps.data[col.data] == 1
          ">
            <Chip label="Đã nhận hàng" icon="pi pi-check" class="bg-success text-white" />
          </template>

          <template v-else-if="
            col.data == 'status_nhanhang' && slotProps.data[col.data] == 2
          ">
            <Chip label="Khiếu nại" icon="pi pi-times" class="bg-danger text-white" />
          </template>

          <template v-else-if="col.data == 'status_nhanhang'">
            <Chip label="Chưa nhận hàng" icon="fas fa-spinner fa-spin" />
          </template>

          <template v-else-if="col.data == 'date_nhanhang' && slotProps.data[col.data]">
            {{ formatDate(slotProps.data[col.data]) }}
          </template>
          <template v-else-if="col.data == 'mahh'">
            {{ slotProps.data["mahh"] }}
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
import Chip from "primevue/chip";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import InputNumber from "primevue/inputnumber";
import Avatar from "primevue/avatar";
import { storeToRefs } from "pinia";

import { formatDate, formatPrice } from "../../utilities/util";
import { useMuahang } from "../../stores/muahang";
import { useAuth } from "../../stores/auth";
import { useGeneral } from "../../stores/general";
import NsxTreeSelect from "../TreeSelect/NsxTreeSelect.vue";

const store_auth = useAuth();
const store_muahang = useMuahang();
const store_general = useGeneral();
const { datatable, model } = storeToRefs(store_muahang);
const { user } = storeToRefs(store_auth);
const minDate = ref(new Date());
const loading = ref(false);
const selected = ref();
const columns = ref([
  {
    label: "STT",
    data: "stt",
    className: "text-center",
  },
  {
    label: "Mã",
    data: "mahh",
    className: "text-center",
  },
  {
    label: "Tên",
    data: "tenhh",
    className: "text-center",
  },
  {
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
    label: "Số lượng nhận hàng",
    data: "soluong_nhanhang",
    className: "text-center",
  },
  {
    label: "Ngày nhận hàng",
    data: "date_nhanhang",
    className: "text-center",
  },
  {
    label: "Ghi chú",
    data: "note_nhanhang",
    className: "text-center",
  },
  {
    label: "Trạng thái",
    data: "status_nhanhang",
    className: "text-center",
  },
  {
    label: "Người nhận hàng",
    data: "user_nhanhang",
    className: "text-center",
  },
]);

const changeProducer = store_general.changeProducer;
const selectedColumns = computed(() => {
  return columns.value.filter((col) => col.hide != true);
});
const changeNhanhang = (row) => {
  if (row.status_nhanhang == 1) {
    row.user_nhanhang_id = user.value.id;
  }
};
const props = defineProps({
  editable: {
    type: Boolean,
    default: false,
  },
});
onMounted(() => {
  // console.log(datatable.value);
});
</script>


<style lang="scss">
#table-muahang-nhanhang {

  .mahh,
  .grade {
    min-width: 150px;
  }

  .tenhh,
  .nhasx,
  .leadtime {
    min-width: 300px;
  }
}
</style>