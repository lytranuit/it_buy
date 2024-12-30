<template>
  <div id="TablemuahangChitiet">
    <DataTable
      showGridlines
      :value="datatable"
      ref="dt"
      id="table-muahang-chitiet"
      class="p-datatable-ct"
      :rowHover="true"
      :loading="loading"
      responsiveLayout="scroll"
      :resizableColumns="true"
      columnResizeMode="expand"
      v-model:selection="selected"
    >
      <!-- <template #header>
                <div class="d-inline-flex" style="width:200px" v-if="!readonly">
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
      <Column
        v-for="col in selectedColumns"
        :field="col.data"
        :header="col.label"
        :key="col.data"
        :showFilterMatchModes="false"
        :class="col.data"
      >
        <template #body="slotProps">
          <template v-if="col.data == 'soluong' && !readonly">
            <InputNumber
              v-model="slotProps.data[col.data]"
              class="p-inputtext-sm"
              :maxFractionDigits="2"
            />
          </template>
          <template v-else-if="col.data == 'note' && !readonly">
            <textarea v-model="slotProps.data[col.data]" class="form-control" />
          </template>
          <template v-else-if="col.data == 'tenhh' && !readonly">
            <input v-model="slotProps.data[col.data]" class="form-control" />
          </template>
          <template v-else-if="col.data == 'grade' && !readonly">
            <input v-model="slotProps.data[col.data]" class="form-control" />
          </template>
          <template v-else-if="col.data == 'nhasx' && !readonly">
            <input v-model="slotProps.data[col.data]" class="form-control" />
          </template>
          <template v-else-if="col.data == 'dvt' && !readonly">
            {{ slotProps.data["dvt"] }}
            <i
              class="fas fa-sync-alt"
              style="cursor: pointer"
              @click="openOp($event, slotProps.data)"
            ></i>
          </template>
          <template v-else-if="col.data == 'mahh' && !readonly">
            <div class="d-flex align-items-center">
              <material-auto-complete
                v-model="slotProps.data[col.data]"
                :type_id="model.type_id"
                @item-select="select($event, slotProps.data)"
              >
              </material-auto-complete>
              <!-- <span class="fas fa-plus ml-3 text-success" style="cursor: pointer;"
                                @click="openNew(slotProps.index)" v-if="model.type_id != 1"></span> -->
            </div>
          </template>
          <template v-else>
            {{ slotProps.data[col.data] }}
          </template>
        </template>
      </Column>
    </DataTable>
    <OverlayPanel ref="op">
      <div>
        Qui đổi từ {{ editingRow.soluong }}
        <input
          class="form-control form-control-sm d-inline-block"
          style="width: 60px; margin-right: 5px"
          v-model="editingRow.dvt"
        />
        mua hàng =
        <input
          class="form-control form-control-sm d-inline-block"
          style="width: 60px; margin-right: 5px"
          v-model="editingRow.soluong_quidoi"
          @change="changeQuidoi(editingRow)"
        />
        <b>{{ editingRow.dvt_dutru }}</b> dự trù (Tỉ lệ 1:{{
          editingRow.quidoi
        }})
      </div>
    </OverlayPanel>
  </div>
</template>
<script setup>
import { onMounted, ref, watch, computed } from "vue";
import Materials from "../../components/TreeSelect/MaterialTreeSelect.vue";
import MaterialAutoComplete from "../AutoComplete/MaterialAutoComplete.vue";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import InputNumber from "primevue/inputnumber";
import OverlayPanel from "primevue/overlaypanel";
import { storeToRefs } from "pinia";

import { useConfirm } from "primevue/useconfirm";
import { rand } from "../../utilities/rand";
import { formatPrice } from "../../utilities/util";
import { useMuahang } from "../../stores/muahang";
import { useGeneral } from "../../stores/general";
import NsxTreeSelect from "../TreeSelect/NsxTreeSelect.vue";

const store_muahang = useMuahang();
const store_general = useGeneral();
const { datatable, model } = storeToRefs(store_muahang);
const { materials } = storeToRefs(store_general);
const editingRow = ref();

const loading = ref(false);
const selected = ref();
const columns = computed(() => {
  if (model.value.type_id == 1) {
    return [
      {
        label: "STT(*)",
        data: "stt",
        className: "text-center",
      },
      {
        label: "Mã(*)",
        data: "mahh",
        className: "text-center",
      },
      {
        label: "Tên(*)",
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
        label: "ĐVT(*)",
        data: "dvt",
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
      },
    ];
  } else {
    return [
      {
        label: "STT(*)",
        data: "stt",
        className: "text-center",
      },
      {
        label: "Mã(*)",
        data: "mahh",
        className: "text-center",
      },
      {
        label: "Tên(*)",
        data: "tenhh",
        className: "text-center",
      },
      {
        label: "ĐVT(*)",
        data: "dvt",
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
      },
    ];
  }
});

const selectedColumns = computed(() => {
  return columns.value.filter((col) => col.hide != true);
});

const changeProducer = store_general.changeProducer;
const op = ref();
const openOp = (event, row) => {
  editingRow.value = row;
  op.value.toggle(event);
};
const select = (event, row) => {
  //   console.log(event);
  var id = event.value.id;
  row.mahh = id;
  // store_general.changeMaterial(row);
};
const readonly = computed(() => {
  if ([9, 10, 11].indexOf(model.value.status_id) != -1) {
    return true;
  }
  return false;
});
const changeQuidoi = (row) => {
  if (row.soluong_quidoi) {
    row.quidoi = row.soluong_quidoi / row.soluong;
  }
};
onMounted(async () => {
  // await store_general.fetchMaterials();
  if ([9, 10, 11].indexOf(model.value.status_id) != -1) {
    readonly.value = true;
  }
});
</script>


<style lang="scss">
#table-muahang-chitiet {
  .mahh,
  .grade {
    min-width: 150px;
  }
  .tenhh,
  .nhasx {
    min-width: 300px;
  }
}
</style>