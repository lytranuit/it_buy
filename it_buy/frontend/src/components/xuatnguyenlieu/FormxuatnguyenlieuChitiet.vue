<template>
  <div id="TablexuatnguyenlieuChitiet">
    <DataTable showGridlines :value="datatable" ref="dt" class="p-datatable-ct" :rowHover="true" :loading="loading"
      responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:selection="selected"
      :rowClass="rowClass">
      <template #header>
        <div class="d-inline-flex" style="width: 200px" v-if="model.status_id == 1">
          <Button label="Thêm" icon="pi pi-plus" class="p-button-success p-button-sm mr-2" @click="addRow"></Button>
          <Button label="Xóa" icon="pi pi-trash" class="p-button-danger p-button-sm"
            :disabled="!selected || !selected.length" @click="confirmDeleteSelected"></Button>
        </div>
        <div class="d-inline-flex float-right"></div>
      </template>

      <template #empty>
        <div class="text-center">Không có dữ liệu.</div>
      </template>
      <Column selectionMode="multiple" v-if="model.status_id == 1"></Column>
      <Column v-for="col in selectedColumns" :field="col.data" :header="col.label" :key="col.data"
        :showFilterMatchModes="false" :class="col.data">
        <template #body="slotProps">
          <template v-if="col.data == 'tenhh' && model.status_id == 1">
            <div class="d-flex align-items-center">
              <NVL-auto-complete v-model="slotProps.data[col.data]" :disabled="slotProps.data['mahh'] != null"
                @item-select="select($event, slotProps.data)">
              </NVL-auto-complete>
              <!-- <span class="fas fa-plus ml-3 text-success" style="cursor: pointer;"
                                @click="openNew(slotProps.index)" v-if="model.type_id != 1"></span> -->
            </div>
          </template>



          <template v-else-if="col.data == 'soluong' && model.status_id == 1">
            <input-number v-model="slotProps.data[col.data]" class="p-inputtext-sm" :maxFractionDigits="2"
              :suffix="' ' + slotProps.data.dvt" />
          </template>

          <template v-else-if="col.data == 'note' && model.status_id == 1">
            <textarea v-model="slotProps.data[col.data]" class="form-control" />
          </template>
          <template v-else-if="col.data == 'dinhkem' && model.status_id == 1">
            <div class="custom-file mt-2">
              <input type="file" class="hinhanh-file-input" :id="'hinhanhFile' + slotProps.data['stt']" :multiple="true"
                :data-key="slotProps.data['stt']" @change="fileChange($event)" />
              <label class="custom-file-label" :for="'hinhanhFile' + slotProps.data['stt']">Choose file</label>
            </div>
            <div class="mt-2 dinhkemchitiet" v-for="(item, index) in slotProps.data['dinhkem']" :key="index">
              <a target="_blank" :href="item.url" class="text-blue" :download="download(item.name)">{{ item.name
              }}</a><i class="text-danger fas fa-trash ml-2" style="cursor: pointer"
                @click="xoachitietdinhkem(index, slotProps.data['dinhkem'])"></i>
            </div>
          </template>

          <template v-else-if="col.data == 'dinhkem'">
            <div class="mt-2 dinhkemchitiet" v-for="(item, index) in slotProps.data['dinhkem']" :key="index">
              <a target="_blank" :href="item.url" class="text-blue" :download="download(item.name)">{{ item.name }}</a>
            </div>
          </template>

          <template v-else-if="col.data == 'tenhh' && model.status_id != 1">
            {{ label(slotProps.data) }}
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
import { useAuth } from "../../stores/auth";
import { usexuatnguyenlieu } from "../../stores/xuatnguyenlieu";
import { useGeneral } from "../../stores/general";
import { useMaterials } from "../../stores/materials";
import { download, formatDate } from "../../utilities/util";
import NVLAutoComplete from "../AutoComplete/NVLAutoComplete.vue";
import xuatnguyenlieuApi from "../../api/xuatnguyenlieuApi";
import { useToast } from "primevue/usetoast";
const store_auth = useAuth();
const { user } = storeToRefs(store_auth);
const toast = useToast();


const rowClass = (data) => {
  return { "bg-gray": data.date_huy ? true : false };
};


const label = (row) => {
  return row.mahh ? row.mahh + " - " + row.tenhh : row.tenhh;
};
const select = (event, row) => {
  row.mahh = event.value.mahh;
  row.tenhh = event.value.tenhh;
  row.dvt = event.value.dvt;
  row.soluong = event.value.soluong;
  row.tonkho = event.value.tonkho;
};
const store_xuatnguyenlieu = usexuatnguyenlieu();
const store_general = useGeneral();
const store_materials = useMaterials();
const RefMaterials = storeToRefs(store_materials);
const modelMaterial = RefMaterials.model;
const { headerForm, visibleDialog } = RefMaterials;

const { datatable, model, list_delete } = storeToRefs(store_xuatnguyenlieu);
const { materials, products } = storeToRefs(store_general);
const confirm = useConfirm();

const loading = ref(false);
const selected = ref();
const columns = computed(() => {

  return [
    {
      label: "STT(*)",
      data: "stt",
      className: "text-center",
    }, {
      label: "Mã nguyên liệu(*)",
      data: "mahh",
      className: "text-center",
    },
    {
      label: "Tên nguyên liệu(*)",
      data: "tenhh",
      className: "text-center",
    },
    {
      label: "Số lượng(*)",
      data: "soluong",
      className: "text-center",
    },
    {
      label: "Mô tả",
      data: "note",
      className: "text-center",
    }
  ];

});
const selectedColumns = computed(() => {
  return columns.value.filter((col) => col.hide != true);
});
const addRow = () => {
  var index = datatable.value.length;
  if (selected.value && selected.value.length == 1) {
    index = selected.value[selected.value.length - 1].stt;
  }
  datatable.value.splice(index, 0, {
    ids: rand(),
    is_new: true,
  });
  calculate_stt();
};
const calculate_stt = () => {
  datatable.value = datatable.value.map((item, index) => {
    item.stt = index + 1;
    return item;
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

<style>
.mahh {
  min-width: 150px;
}

.tenhh,
.dinhkem {
  min-width: 300px;
  word-wrap: break-word;
  white-space: break-spaces;
}

.note {
  word-wrap: break-word;
  white-space: pre-line;
}



.note {
  min-width: 150px;
}

tr.bg-gray {
  background-color: rgb(229 229 229) !important;
}
</style>
