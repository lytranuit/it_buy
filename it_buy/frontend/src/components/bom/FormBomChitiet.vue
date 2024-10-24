<template>
  <div id="TableChitiet">
    <DataTable
      showGridlines
      :value="datatable"
      ref="dt"
      class="p-datatable-ct"
      :rowHover="true"
      :loading="loading"
      responsiveLayout="scroll"
      :resizableColumns="true"
      columnResizeMode="expand"
      v-model:selection="selected"
    >
      <template #header>
        <div class="d-inline-flex">
          <Button
            label="Thêm"
            icon="pi pi-plus"
            class="p-button-success p-button-sm mr-2"
            @click="addRow"
          ></Button>
          <Button
            label="Xóa"
            icon="pi pi-trash"
            class="p-button-danger p-button-sm mr-2"
            :disabled="!selected || !selected.length"
            @click="confirmDeleteSelected"
          ></Button>
          <Button
            label="Thêm NVL thay thế"
            icon="pi pi-plus"
            class="p-button-warning p-button-sm"
            :disabled="!selected || selected.length != 1"
            @click="addThayThe"
          ></Button>
        </div>
        <div class="d-inline-flex float-right"></div>
      </template>

      <template #empty>
        <div class="text-center">Không có dữ liệu.</div>
      </template>
      <Column selectionMode="multiple"></Column>
      <Column
        v-for="col in selectedColumns"
        :field="col.data"
        :header="col.label"
        :key="col.data"
        :showFilterMatchModes="false"
        :class="col.data"
      >
        <template #body="slotProps">
          <template v-if="col.data == 'manvl'">
            <div class="d-flex align-items-center">
              <material-auto-complete
                v-model="slotProps.data[col.data]"
                @item-select="select($event, slotProps.data)"
              >
              </material-auto-complete>
            </div>
            <template v-if="filter(slotProps.data.thaythe)">
              <div v-for="item in filter(slotProps.data.thaythe)" :key="item">
                <div class="d-flex justify-content-center my-2">
                  <i class="fas fa-exchange-alt"></i>
                  <i
                    class="fas fa-trash-alt ml-2 text-danger"
                    @click="confirmRemoveThaythe(item)"
                    style="cursor: pointer"
                  ></i>
                </div>
                <div class="d-flex align-items-center">
                  <material-auto-complete
                    v-model="item.manvl"
                    @item-select="select($event, item)"
                  >
                  </material-auto-complete>
                </div>
              </div>
            </template>
          </template>

          <template v-else-if="col.data == 'dvtnvl' || col.data == 'tennvl'">
            <input
              v-model="slotProps.data[col.data]"
              class="p-inputtext p-inputtext-sm w-100"
              required
            />
            <template v-if="filter(slotProps.data.thaythe)">
              <div v-for="item in filter(slotProps.data.thaythe)" :key="item">
                <div class="d-flex justify-content-center my-2">
                  <i class="fas fa-exchange-alt"></i>
                </div>
                <div class="d-flex align-items-center">
                  <input
                    v-model="item[col.data]"
                    class="p-inputtext p-inputtext-sm w-100"
                    required
                  />
                </div>
              </div>
            </template>
          </template>
          <template v-else-if="col.data == 'me' || col.data == 'soluong'">
            <input-number
              v-model="slotProps.data[col.data]"
              class="w-100"
              :maxFractionDigits="2"
            />
            <template v-if="filter(slotProps.data.thaythe)">
              <div v-for="item in filter(slotProps.data.thaythe)" :key="item">
                <div class="d-flex justify-content-center my-2">
                  <i class="fas fa-exchange-alt"></i>
                </div>
                <div class="d-flex align-items-center">
                  <input-number
                    v-model="item[col.data]"
                    class="w-100"
                    :maxFractionDigits="2"
                  />
                </div>
              </div>
            </template>
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
import Button from "primevue/button";
import InputNumber from "primevue/inputnumber";
import { storeToRefs } from "pinia";

import { useConfirm } from "primevue/useconfirm";
import { rand } from "../../utilities/rand";
import { useBom } from "../../stores/Bom";
import MaterialAutoComplete from "../AutoComplete/MaterialAutoComplete.vue";
import { download, formatDate } from "../../utilities/util";
import { useToast } from "primevue/usetoast";
const toast = useToast();

const select = (event, row) => {
  row.manvl = event.value.mahh;
  row.tennvl = event.value.tenhh;
  row.dvtnvl = event.value.dvt;
};
const filter = (rows) => {
  return rows.filter((item) => {
    return item.deleted != true;
  });
};

const store_Bom = useBom();
// const store_general = useGeneral();
const { datatable, model, list_delete, list_delete_thaythe } =
  storeToRefs(store_Bom);
// const { materials } = storeToRefs(store_general);
const confirm = useConfirm();

const loading = ref(false);
const selected = ref();
const columns = computed(() => {
  return [
    {
      label: "STT(*)",
      data: "stt",
      className: "text-center",
    },
    {
      label: "Mã NVL(*)",
      data: "manvl",
      className: "text-center",
    },
    {
      label: "Tên NVL(*)",
      data: "tennvl",
      className: "text-center",
    },
    {
      label: "ĐVT(*)",
      data: "dvtnvl",
      className: "text-center",
    },
    {
      label: "Mẻ(*)",
      data: "me",
      className: "text-center",
    },
    {
      label: "Số lượng(*)",
      data: "soluong",
      className: "text-center",
    },
  ];
});
const selectedColumns = computed(() => {
  return columns.value.filter((col) => col.hide != true);
});
const addRow = () => {
  let stt = 0;
  if (datatable.value.length) {
    stt = datatable.value[datatable.value.length - 1].stt;
  }
  stt++;
  datatable.value.push({ ids: rand(), stt: stt, me: 1, thaythe: [] });
};
const addThayThe = () => {
  var se = selected.value[0];

  var stt = 0;
  if (se.thaythe && se.thaythe.length > 0) {
    stt = se.thaythe.length + 1;
  } else {
    se.thaythe = [];
    stt = 1;
  }
  se.thaythe.push({ ids: rand(), stt: stt, me: 1 });
  console.log(se);
};
const confirmRemoveThaythe = (item) => {
  confirm.require({
    message: "Bạn có chắc muốn xóa những dòng này?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      // console.log(row.thaythe);
      item.deleted = true;
    },
  });
};
const confirmDeleteSelected = () => {
  confirm.require({
    message: "Bạn có chắc muốn xóa những dòng đã chọn?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      datatable.value = datatable.value.filter((item) => {
        return !selected.value.includes(item);
      });

      if (!list_delete.value) {
        list_delete.value = [];
      }
      for (var item of selected.value) {
        if (!item.ids) {
          list_delete.value.push(item);
        }
      }
      selected.value = [];
      // selected
    },
  });
};
onMounted(async () => {
  // if(items)
});
</script>

