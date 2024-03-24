<template>
  <AutoComplete :modelValue="modelValue" @update:modelValue="emit('update:modelValue', $event)" optionLabel="label"
    optionGroupLabel="label" optionGroupChildren="items" :disabled="disabled" :suggestions="filterItems"
    @complete="search" class="w-100" inputClass="form-control form-control-sm"
    @item-select="emit('item-select', $event)">
    <template #option="slotProps">
      <div class="flex align-options-center">
        {{ slotProps.option.mahh }} - {{ slotProps.option.tenhh }}
      </div>
    </template>
  </AutoComplete>
</template>

<script setup>
import { storeToRefs } from "pinia";
import { computed, onMounted, ref } from "vue";
import AutoComplete from 'primevue/autocomplete';
import { useGeneral } from '../../stores/general'
import { FilterMatchMode, FilterService } from "primevue/api";
const props = defineProps({
  modelValue: {
    type: [String],
  },
  type_id: {
    type: [Number, String],
  },
  disabled: {
    type: Boolean,
    default: false,
  },
});
const emit = defineEmits(["update:modelValue", "item-select"]);
const store = useGeneral();
const { materialGroup } = storeToRefs(store);

const items = computed(() => {

  return materialGroup.value.map((item) => {
    item.label = item.manhom + " - " + item.tennhom;
    item.items = item.items.map((i) => {
      i.label = i.mahh + "-" + i.tenhh;
      return i;
    });
    return item;
  });
})
const filterItems = ref([]);

const search = (event) => {
  let query = event.query;
  let newFiltered = [];

  for (let item of items.value) {
    if (props.type_id != 1) {
      if (["0721", "0722", "0723", "0724", "0725", "Khac"].indexOf(item.manhom) == -1) {
        continue;
      }
    } else {
      if (["0721", "0722", "0723", "0724", "0725", "Khac"].indexOf(item.manhom) != -1) {
        continue;
      }
    }
    let filteredItems = FilterService.filter(item.items, ['label'], query, FilterMatchMode.CONTAINS);
    if (filteredItems && filteredItems.length) {
      newFiltered.push({ ...item, ...{ items: filteredItems } });
    }
  }

  filterItems.value = newFiltered;

}
onMounted(() => {
  store.fetchMaterialGroup();
});
</script>
